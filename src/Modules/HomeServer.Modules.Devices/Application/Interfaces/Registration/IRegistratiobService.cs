using HomeServer.Module.Devices.Application.Dto;
using HomeServer.Modules.Devices.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IRegistrationService
{
    Task<Guid> ReceiveAsync(
        DeviceTransport transport,
        string payload,
        string? remoteAddress,
        CancellationToken cancellationToken);

    IReadOnlyCollection<PendingRegistrationDto> GetAll();

    PendingRegistrationDto? Get(Guid id);
    Task<PendingRegistrationDto?> AnalyzeAsync(Guid id, DeviceAdapter adapter, CancellationToken cancellationToken);
    Task<bool> CompleteAsync(Guid id, RegistrationCompleteRequest request, CancellationToken cancellationToken);
}