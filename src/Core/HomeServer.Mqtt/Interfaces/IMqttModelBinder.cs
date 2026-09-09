using HomeServer.Mqtt.Models;
using MQTTnet;

namespace HomeServer.Mqtt.Interfaces;

public interface IMqttModelBinder
{
    object?[] Bind(
        MqttRouteDescriptor descriptor,
        MqttApplicationMessageReceivedEventArgs args,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}