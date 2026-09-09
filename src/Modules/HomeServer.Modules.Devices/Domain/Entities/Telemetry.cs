using HomeServer.Persistence.Interfeaces;

namespace HomeServer.Modules.Devices.Domain.Entities;

public class Telemetry : IDbEntity
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Device Device { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string ValueType { get; set; } = null!;

    public string? Unit { get; set; }

    public string? ConfigurationJson { get; set; }
}