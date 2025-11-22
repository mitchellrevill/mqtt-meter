using System.Linq;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Server.Messaging;

namespace Infrastructure.IntegrationTests.Messaging;

public class MqttReadingProcessorServiceTests
{
    [Fact]
    public async Task ReadingMessage_SavesReading_AndPublishesBillingSnapshot()
    {
        const string ReadingsBaseTopic = "meters/readings";
        const string BillingBaseTopic = "meters/billing";

        var subscriber = new TestMqttSubscriber();
        var publisher = new TestMqttPublisher();
        var readingService = new TestReadingService();
        var scopeFactory = new TestScopeFactory(readingService);
        var options = Options.Create(new MqttTopicOptions());

        var service = new MqttReadingProcessorService(
            NullLogger<MqttReadingProcessorService>.Instance,
            subscriber,
            publisher,
            scopeFactory,
            options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await service.StartAsync(cts.Token);
        await subscriber.WaitForSubscriptionAsync("meters/readings/#", TimeSpan.FromSeconds(1));

        var topic = $"{ReadingsBaseTopic}/user-123";
        await subscriber.TriggerAsync(topic, new { clientId = "user-123", value = 42.5 });

        await TestWaiter.WaitUntilAsync(() => readingService.Readings.Count == 1, TimeSpan.FromSeconds(1));
        await TestWaiter.WaitUntilAsync(() => publisher.PublishedMessages.Count == 1, TimeSpan.FromSeconds(1));

        readingService.Readings.Should().ContainSingle(r => r.UserId == "user-123" && Math.Abs(r.Value - 42.5f) < 0.001f);

        var message = publisher.PublishedMessages.Single();
        message.Topic.Should().Be($"{BillingBaseTopic}/user-123");
        message.Payload.RootElement.GetProperty("TotalKwhUsed").GetDouble().Should().Be(42.5);
        message.Payload.RootElement.GetProperty("TotalAmount").GetDouble().Should().BeApproximately(42.5 * 0.15, 0.0001);
        message.Payload.RootElement.GetProperty("ReadingCount").GetInt32().Should().Be(1);

        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task BillingResetCommand_ResetsReadings_AndPublishesBillingUpdate()
    {
        const string BillingBaseTopic = "meters/billing";
        var subscriber = new TestMqttSubscriber();
        var publisher = new TestMqttPublisher();
        var readingService = new TestReadingService();
        var scopeFactory = new TestScopeFactory(readingService);
        var options = Options.Create(new MqttTopicOptions());

        var service = new MqttReadingProcessorService(
            NullLogger<MqttReadingProcessorService>.Instance,
            subscriber,
            publisher,
            scopeFactory,
            options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await service.StartAsync(cts.Token);
        await subscriber.WaitForSubscriptionAsync($"{BillingBaseTopic}/#", TimeSpan.FromSeconds(1));

        var topic = $"{BillingBaseTopic}/user-123/reset";
        await subscriber.TriggerAsync(topic, new { command = "reset" });

        await TestWaiter.WaitUntilAsync(() => readingService.ResetCalls.Contains("user-123"), TimeSpan.FromSeconds(1));
        await TestWaiter.WaitUntilAsync(() => publisher.PublishedMessages.Count > 0, TimeSpan.FromSeconds(1));

        readingService.ResetCalls.Should().ContainSingle(call => call == "user-123");

        var message = publisher.PublishedMessages.Single();
        message.Topic.Should().Be($"{BillingBaseTopic}/user-123");
        message.Payload.RootElement.GetProperty("UserId").GetString().Should().Be("user-123");
        message.Payload.RootElement.GetProperty("TotalKwhUsed").GetDouble().Should().Be(0.0);
        message.Payload.RootElement.GetProperty("TotalAmount").GetDouble().Should().Be(0.0);
        message.Payload.RootElement.GetProperty("ReadingCount").GetInt32().Should().Be(0);

        cts.Cancel();
        await service.StopAsync(CancellationToken.None);
    }

    private sealed class TestMqttSubscriber : IMqttSubscriber
    {
        private readonly Dictionary<string, Func<string, byte[], Task>> _handlers = new();

        public bool IsConnected => true;

        public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SubscribeAsync(string topic, Func<string, byte[], Task> messageHandler, CancellationToken cancellationToken = default)
        {
            _handlers[topic] = messageHandler;
            return Task.CompletedTask;
        }

        public Task SubscribeAsync<T>(string topic, Func<string, T, Task> messageHandler, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SubscribeSharedAsync(string groupName, string topic, Func<string, byte[], Task> messageHandler, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SubscribeSharedAsync<T>(string groupName, string topic, Func<string, T, Task> messageHandler, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UnsubscribeAsync(string topic, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public async Task WaitForSubscriptionAsync(string topic, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < timeout)
            {
                if (_handlers.ContainsKey(topic))
                {
                    return;
                }

                await Task.Delay(25, CancellationToken.None);
            }

            throw new TimeoutException($"Subscription for topic '{topic}' was not registered in time.");
        }

        public async Task TriggerAsync(string topic, object payload)
        {
            var handler = _handlers.FirstOrDefault(pair => TopicMatches(pair.Key, topic)).Value
                ?? throw new InvalidOperationException($"No handler registered for topic '{topic}'.");

            var json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);
            await handler(topic, body);
        }

        private static bool TopicMatches(string subscription, string topic)
        {
            if (subscription.EndsWith("/#", StringComparison.Ordinal))
            {
                var prefix = subscription[..^2];
                return topic.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            return subscription.Equals(topic, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class TestMqttPublisher : IMqttPublisher
    {
        public List<PublishedMessage> PublishedMessages { get; } = new();

        public bool IsConnected { get; set; } = true;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishAsync<T>(string topic, T payload, bool retain = false, CancellationToken cancellationToken = default)
        {
            var document = JsonDocument.Parse(JsonSerializer.Serialize(payload));
            PublishedMessages.Add(new PublishedMessage(topic, document));
            return Task.CompletedTask;
        }

        public Task PublishAsync(string topic, byte[] payload, bool retain = false, CancellationToken cancellationToken = default)
        {
            var json = Encoding.UTF8.GetString(payload);
            var document = JsonDocument.Parse(json);
            PublishedMessages.Add(new PublishedMessage(topic, document));
            return Task.CompletedTask;
        }
    }

    private sealed class TestReadingService : IReadingService
    {
        private readonly List<Reading> _readings = new();

        public List<string> ResetCalls { get; } = new();

        public IReadOnlyList<Reading> Readings => _readings;

        public Task<Reading> CreateAsync(string userId, float value)
        {
            var reading = new Reading { UserId = userId, Value = value, TimeStamp = DateTime.UtcNow };
            _readings.Add(reading);
            return Task.FromResult(reading);
        }

        public Task<IEnumerable<Reading>> GetAll()
            => Task.FromResult<IEnumerable<Reading>>(_readings.ToList());

        public Task<IEnumerable<Reading>> GetByUserId(string userId)
        {
            var filtered = _readings.Where(r => r.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult<IEnumerable<Reading>>(filtered);
        }

        public Task ResetForUserAsync(string userId)
        {
            ResetCalls.Add(userId);
            _readings.RemoveAll(r => r.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));
            return Task.CompletedTask;
        }
    }

    private sealed class TestScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TestScopeFactory(IReadingService readingService)
        {
            _serviceProvider = new TestServiceProvider(readingService);
        }

        public IServiceScope CreateScope() => new TestScope(_serviceProvider);

        private sealed class TestScope : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }

            public TestScope(IServiceProvider provider)
            {
                ServiceProvider = provider;
            }

            public void Dispose()
            {
            }
        }

        private sealed class TestServiceProvider : IServiceProvider
        {
            private readonly IReadingService _readingService;

            public TestServiceProvider(IReadingService readingService)
            {
                _readingService = readingService;
            }

            public object? GetService(Type serviceType)
                => serviceType == typeof(IReadingService) ? _readingService : null;
        }
    }

    private sealed record PublishedMessage(string Topic, JsonDocument Payload);

    private static class TestWaiter
    {
        public static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < timeout)
            {
                if (condition())
                {
                    return;
                }

                await Task.Delay(25);
            }

            throw new TimeoutException("Condition was not satisfied before timeout.");
        }
    }
}
