using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Server.Messaging;

public class RabbitMqOptions
{
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ReadingsQueue { get; set; } = string.Empty;
}

public class RabbitMqConnection : IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqConnection(RabbitMqOptions options)
    {
        _options = options;
        
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.Username,
            Password = _options.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declare the queue
        _channel.QueueDeclare(
            queue: _options.ReadingsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void Publish(ReadingMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: "",
            routingKey: _options.ReadingsQueue,
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}