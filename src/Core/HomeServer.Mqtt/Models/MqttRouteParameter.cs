namespace HomeServer.Mqtt.Models;

public sealed class MqttRouteParameter
{
    public required string Name { get; init; }

    public required Type Type { get; init; }
}