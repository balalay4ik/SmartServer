using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public class ConnectedDevice
{
    public string ExternalId { get; set; } = null!;
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
    public Dictionary<string, string> Command { get; set; } = new();
    public Dictionary<string, string> Telemetries { get; set; } = new();
    public object? Object { get; set; }
}