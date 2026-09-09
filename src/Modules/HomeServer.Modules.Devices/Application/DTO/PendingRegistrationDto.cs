using HomeServer.Modules.Devices.Application.Dto;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Module.Devices.Application.Dto;

public sealed class PendingRegistrationDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    public DeviceTransport Transport { get; set; }

    public DeviceAdapter? Adapter { get; set; }

    public IReadOnlyCollection<PendingRegistrationMessageDto> Messages { get; set; } = [];
}