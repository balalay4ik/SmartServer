using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public class DeviceDescriptor
{
    // Идентификация
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DeviceStatus Status { get; set; }


    // HomeServer
    public DeviceAdapter? Adapter { get; set; }

    public List<CommandDescriptor> Commands { get; set; } = [];

    public List<TelemetryDescriptor> Telemetries { get; set; }
        = [];

    public DeviceInfoDescriptor? Info { get; set; }
}