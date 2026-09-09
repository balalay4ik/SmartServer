using HomeServer.Domain.Entities;

namespace HomeServer.Modules.Devices.Application.Models;

public sealed class DeviceWarning
{
    public required DeviceWarningType Type { get; init; }

    public required string Message { get; init; }

    public WarningSeverity Severity { get; init; }
}