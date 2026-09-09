using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Persistence.Interfeaces;
using HomeServer.SDK.Devices.Entity;

namespace HomeServer.Modules.Devices.Domain.Entities;

public class Device : IDbEntity, IDeviceEntity
{
    public Guid Id { get; set; }

    public string ExternalId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DeviceTransport Transport { get; set; }

    public DeviceAdapter Adapter { get; set; }

    public string Address { get; set; } = null!;

    public ICollection<Command> Commands { get; set; }
        = new List<Command>();

    public ICollection<Telemetry> Telemetries { get; set; }
        = new List<Telemetry>();

    public Guid? InfoId { get; set; }
    public DeviceInfo? Info { get; set; }
}