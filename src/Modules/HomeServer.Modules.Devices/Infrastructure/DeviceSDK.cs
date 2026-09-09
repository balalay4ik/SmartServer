using System.Threading.Tasks;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Domain.Entities;
using HomeServer.Persistence.Interfeaces;
using HomeServer.SDK.Devices;
using Microsoft.EntityFrameworkCore;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public class DeviceSDK : IDeviceSDK
{
    private readonly IRepositoryFactory _repository;
    private readonly IDeviceService _deviceService;

    public DeviceSDK(
        IRepositoryFactory repository,
        IDeviceService deviceService
    )
    {
        _repository = repository;
        _deviceService = deviceService;
    }

    public async Task<IEnumerable<string>> GetExternalIdAllRegisteredDevice()
    {
        return await _repository.Table<Device>().QueryAsync(q => q.Select(x => x.ExternalId).ToListAsync());
    }

    public async Task SendCommandByDbId(Guid id, string key, object? value, CancellationToken cancellationToken)
    {
        await _deviceService.SendCommandAsync(id, new Application.Models.DeviceCommandRequest() { Key = key, Value = value }, cancellationToken);
    }

    public async Task SendCommandByExternalId(string id, string key, object? value, CancellationToken cancellationToken)
    {
        var dbId = (await _repository.Table<Device>().QueryAsync(q => q.FirstAsync(x => x.ExternalId == id), cancellationToken)).Id;
        await _deviceService.SendCommandAsync(dbId, new Application.Models.DeviceCommandRequest() { Key = key, Value = value }, cancellationToken);
    }
}