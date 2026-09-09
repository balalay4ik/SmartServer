using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class DeviceTransportFactory : IDeviceTransportFactory
{
    private readonly IEnumerable<IDeviceTransportStrategy> _strategies;

    public DeviceTransportFactory(
        IEnumerable<IDeviceTransportStrategy> strategies
    )
    {
        _strategies = strategies;
    }

    public IDeviceTransportStrategy Create(DeviceTransport transport)
    {
        return _strategies.FirstOrDefault(
            x => x.Transport == transport)
            ?? throw new NotSupportedException(
                $"Transport '{transport}' is not supported.");
    }
}