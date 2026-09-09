using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDeviceTransportFactory
{
    IDeviceTransportStrategy Create(DeviceTransport transport);

}