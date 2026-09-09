using HomeServer.Application.Models;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public sealed class RegistrationContext
{
    public Guid Id { get; } = Guid.NewGuid();

    public required DeviceTransport Transport { get; init; }

    public required string? Address { get; init; }

    public required string RawPayload { get; init; }

    public DeviceAdapter? Adapter { get; set; }

    public AdapterAnalyzeResult? AnalyzeResult { get; set; }

    public AdapterMatchType MatchType { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiredAt { get; init; } = DateTime.UtcNow.AddDays(1);
}