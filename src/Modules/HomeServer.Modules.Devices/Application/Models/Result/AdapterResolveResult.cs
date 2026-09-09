using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Application.Models;

public sealed class AdapterResolveResult
{
    public required DeviceAdapter Adapter { get; init; }

    public required AdapterMatchType MatchType { get; init; }
}