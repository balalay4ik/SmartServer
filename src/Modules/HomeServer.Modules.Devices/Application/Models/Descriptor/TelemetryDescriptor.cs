namespace HomeServer.Modules.Devices.Application.Models;

public class TelemetryDescriptor
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ValueType { get; set; } = null!;

    public string? Unit { get; set; }
    public string? Value { get; set; }

    public Dictionary<string, object>? Configuration { get; set; }
}