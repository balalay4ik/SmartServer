
using System.Text.Json;
using HomeServer.Application.Models.Event;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Entities;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DiscoveredDevice>> DiscoverDevicesAsync();

    Task<List<Device>> GetDevices(CancellationToken cancellationToken = default);
    Task<DeviceDescriptor?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<Device?> RegisterAsync(RegistrationContext context, CancellationToken cancellationToken);
    Task SendCommandAsync(Guid id, DeviceCommandRequest request, CancellationToken cancellationToken);
    Task Availabality(JsonElement payload, CancellationToken cancellationToken);
    Task Response(JsonElement payload, CancellationToken cancellationToken);
}