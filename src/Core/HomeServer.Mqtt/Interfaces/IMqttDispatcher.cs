using MQTTnet;

namespace HomeServer.Mqtt.Interfaces;

public interface IMqttDispatcher
{
    Task DispatchAsync(
        MqttApplicationMessageReceivedEventArgs args,
        CancellationToken cancellationToken = default);
}