using MQTTnet;

namespace HomeServer.Mqtt.Interfaces;

public interface IMqttService
{
    bool IsConnected { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task ShutdownAsync(CancellationToken cancellationToken = default);

    Task PublishAsync(
    MqttApplicationMessage message,
    CancellationToken cancellationToken = default);
}