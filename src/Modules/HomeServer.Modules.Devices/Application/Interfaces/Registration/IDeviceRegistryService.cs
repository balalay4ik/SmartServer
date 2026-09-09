
using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDeviceRegistryService
{
    Task RegisterAsync(
        DeviceRegistrationContext context,
        CancellationToken cancellationToken);
}