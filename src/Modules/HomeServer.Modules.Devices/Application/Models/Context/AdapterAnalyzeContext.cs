using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public class AdapterAnalyzeContext
{
    public required string Payload { get; init; }
    public required string? Address { get; set; }
    public required DeviceTransport Transport { get; init; }
    public CancellationToken CancellationToken { get; init; }
}