using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class DeviceAdapterFactory : IDeviceAdapterFactory
{
    private readonly IEnumerable<IDeviceAdapterStrategy> _strategies;

    public DeviceAdapterFactory(
        IEnumerable<IDeviceAdapterStrategy> strategies
    )
    {
        _strategies = strategies;
    }

    public IDeviceAdapterStrategy Create(DeviceAdapter adapter)
    {
        return _strategies.FirstOrDefault(
            x => x.Adapter == adapter)
            ?? throw new NotSupportedException(
                $"Adapter '{adapter}' is not supported.");
    }
}