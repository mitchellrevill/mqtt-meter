namespace MqttTestClient.Options;

public class TestClientOptions
{
    public const string SectionName = "TestClient";

    private string _clientId = string.Empty;

    /// <summary>
    /// Meter identifier. If not set, defaults to "meter-{hostname}" for unique IDs in scaled deployments.
    /// </summary>
    public string ClientId
    {
        get => string.IsNullOrEmpty(_clientId)
            ? $"meter-{Environment.MachineName.ToLowerInvariant()}"
            : _clientId;
        set => _clientId = value;
    }

    public int PublishIntervalSeconds { get; set; } = 5;
    public double MinimumValue { get; set; } = 0.2;
    public double MaximumValue { get; set; } = 2.0;
    public string Unit { get; set; } = "kWh";
    public bool PublishStatusMessages { get; set; } = true;
}
