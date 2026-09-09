using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Infrastructure.Transport;

public class WebSocketTransportStrategy : IDeviceTransportStrategy
{
    public DeviceTransport Transport => DeviceTransport.WebSocket;

    public Task Ping(TransportRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> SendAsync(TransportRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}