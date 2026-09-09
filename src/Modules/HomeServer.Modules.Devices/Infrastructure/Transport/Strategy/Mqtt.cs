using System.Buffers;
using System.Text;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Mqtt.Interfaces;
using MQTTnet;
using MQTTnet.Protocol;

namespace HomeServer.Infrastructure.Transport;

public class MqttTransportStrategy : IDeviceTransportStrategy
{
    public DeviceTransport Transport => DeviceTransport.Mqtt;

    private readonly IMqttClient _client;

    public MqttTransportStrategy(
        IMqttClient client
    )
    {
        _client = client;
    }

    public async Task<string> SendAsync(
        TransportRequest request,
        CancellationToken cancellationToken)
    {
        var message = GetMessage(request);

        if (request.Metadata.TryGetValue("QoS_1", out var qos))
        {
            message.QualityOfServiceLevel =
                Enum.Parse<MqttQualityOfServiceLevel>(qos);
        }

        if (request.Metadata.TryGetValue("Retain", out var retain))
        {
            message.Retain = bool.Parse(retain);
        }

        await _client.PublishAsync(message, cancellationToken);

        return string.Empty;
    }

    private static MqttApplicationMessage GetMessage(TransportRequest request)
    {
        var payload = Encoding.UTF8.GetBytes(request.Message);

        var message = new MqttApplicationMessage
        {
            Topic = request.Address,
            Payload = new ReadOnlySequence<byte>(payload)
        };
        return message;
    }

    public async Task Ping(TransportRequest request, CancellationToken cancellationToken)
    {
        var message = GetMessage(request);

        await _client.PublishAsync(message, cancellationToken);
    }
}