using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public sealed class DescriptorChange
{
    public required DescriptorChangeType Type { get; init; }

    public string? Key { get; init; }

    public required string Property { get; init; }

    public DescriptorChangeAction Action { get; init; }

    public object? OldValue { get; init; }

    public object? NewValue { get; init; }

    public bool AutoApply { get; init; }
}