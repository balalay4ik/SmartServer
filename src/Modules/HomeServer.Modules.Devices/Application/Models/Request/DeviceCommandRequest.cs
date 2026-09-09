namespace HomeServer.Modules.Devices.Application.Models;

public sealed class DeviceCommandRequest
{
    public string Key { get; init; } = null!;

    public object? Value { get; init; }
}