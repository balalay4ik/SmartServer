using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Application.Models.Event;

public class StatusChangeEvent
{
    public string ExternalId { get; set; } = null!;
    public DeviceStatus Status { get; set; }
    public Dictionary<string, string> Commands { get; set; } = new();
    public Dictionary<string, string> Telemetry { get; set; } = new();
}