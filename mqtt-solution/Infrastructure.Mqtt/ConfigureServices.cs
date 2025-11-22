using Application.Interfaces.Repositories;
using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.DatabaseContext;
using Infrastructure.Mqtt.Interfaces;
using Infrastructure.Mqtt.Repositories;
using Infrastructure.Mqtt.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Mqtt;

/// <summary>
/// Extension methods for configuring MQTT services
/// </summary>
public static class ConfigureServices
{
    /// <summary>
    /// Add RabbitMQ MQTT services to the service collection
    /// </summary>
    public static IServiceCollection AddRabbitMqMqtt(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options using OptionsConfigurationServiceCollectionExtensions
        var rabbitMqSection = configuration.GetSection(RabbitMqOptions.SectionName);
        var topicSection = configuration.GetSection(MqttTopicOptions.SectionName);
        
        services.Configure<RabbitMqOptions>(rabbitMqSection);
        services.Configure<MqttTopicOptions>(topicSection);

        // Register services
        services.AddSingleton<IMqttPublisher, MqttPublisher>();
        services.AddSingleton<IMqttSubscriber, MqttSubscriber>();

        // Register background service for managing subscriber lifecycle
        services.AddHostedService<MqttBackgroundService>();

        return services;
    }
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddDbContext<MqttDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemory");
                });

                services.AddScoped<ISampleRepository, SampleRepository>();
                services.AddScoped<IReadingRepository, ReadingRepository>();
                services.AddScoped<IClientRepository, ClientRepository>();

                return services;
            }
    /// <summary>
    /// Add RabbitMQ MQTT services with custom configuration
    /// </summary>
    public static IServiceCollection AddRabbitMqMqtt(
        this IServiceCollection services,
        Action<RabbitMqOptions> configureRabbitMq,
        Action<MqttTopicOptions>? configureMqttTopics = null)
    {
        // Configure options with actions
        services.Configure(configureRabbitMq);
        
        if (configureMqttTopics != null)
        {
            services.Configure(configureMqttTopics);
        }

        // Register services
        services.AddSingleton<IMqttPublisher, MqttPublisher>();
        services.AddSingleton<IMqttSubscriber, MqttSubscriber>();

        // Register background service
        services.AddHostedService<MqttBackgroundService>();

        return services;
    }
}
