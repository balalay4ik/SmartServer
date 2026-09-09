using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IPendingCommandStore
{
    Task<Guid> AddAsync(
    PendingCommand command,
    CancellationToken cancellationToken = default);

    Task<PendingCommand?> GetByCommandIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PendingCommand?> GetByDeviceIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PendingCommand?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}