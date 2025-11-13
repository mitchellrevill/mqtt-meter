namespace Infrastructure.Mqtt.Configuration;

/// <summary>
/// Configuration for MQTT topics
/// </summary>
public class MqttTopicOptions
{
    public const string SectionName = "MqttTopics";

    /// <summary>
    /// Base topic for all meter readings
    /// </summary>
    public string ReadingsBaseTopic { get; set; } = "meters/readings";

    /// <summary>
    /// Topic for client status updates
    /// </summary>
    public string ClientStatusTopic { get; set; } = "meters/clients/status";

    /// <summary>
    /// Topic for system commands
    /// </summary>
    public string CommandTopic { get; set; } = "meters/commands";

    /// <summary>
    /// Topic for alerts and notifications
    /// </summary>
    public string AlertTopic { get; set; } = "meters/alerts";

    /// <summary>
    /// Gets the full topic path for a specific meter reading
    /// </summary>
    public string GetMeterReadingTopic(string clientId) => $"{ReadingsBaseTopic}/{clientId}";

    /// <summary>
    /// Gets the shared subscription topic for readings
    /// </summary>
    public string GetSharedReadingsTopic(string groupName) => $"$share/{groupName}/{ReadingsBaseTopic}/#";

    /// <summary>
    /// Gets the wildcard topic for all readings
    /// </summary>
    public string GetAllReadingsTopic() => $"{ReadingsBaseTopic}/#";
}
