using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDeviceTransportStrategy
{
    DeviceTransport Transport { get; }

    Task Ping(TransportRequest request, CancellationToken cancellationToken);
    Task<string> SendAsync(
        TransportRequest request,
        CancellationToken cancellationToken);
}