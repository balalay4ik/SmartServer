using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDeviceAdapterFactory
{
    IDeviceAdapterStrategy Create(DeviceAdapter adapter);
}