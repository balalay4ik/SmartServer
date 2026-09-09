using HomeServer.Application.Models.Event;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Interfaces.Events;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Modules.Devices.Infrastructure.Services.Events;

namespace HomeServer.Infrastructure.Transport;

public class PendingCommandStore : IPendingCommandStore
{
    private readonly IDeviceEvents _deviceEvents;

    private readonly Dictionary<Guid, PendingCommand> _commands = new();

    public PendingCommandStore(
        IDeviceEvents deviceEvents
    )
    {
        _deviceEvents = deviceEvents;

        _deviceEvents.DeviceStatusChanged += OnDeviceStatusChanged;
    }

    public Task<Guid> AddAsync(
        PendingCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = command.Id;

        _commands[id] = command;

        return Task.FromResult(id);
    }

    public async Task<PendingCommand?> GetByCommandIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var p = _commands.FirstOrDefault(x => x.Value.CommandId == id).Value;
        return p;
    }

    public async Task<PendingCommand?> GetByDeviceIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var p = _commands.FirstOrDefault(x => x.Value.DeviceId == id).Value;
        return p;
    }

    public Task<PendingCommand?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _commands.TryGetValue(id, out var command);

        return Task.FromResult(command);
    }

    public Task RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _commands.Remove(id);

        return Task.CompletedTask;
    }

    private async Task OnDeviceStatusChanged(StatusChangeEvent @event)
    {
        if (@event.Status == DeviceStatus.Online) return;
        var p = _commands.FirstOrDefault(x => x.Value.ExternalId == @event.ExternalId).Value;
        if (p is null) return;
        _commands.Remove(p.Id);
    }
}