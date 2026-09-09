using System.Reflection.Metadata;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public class DiscoveredDevice
{
    public string Address { get; set; } = null!;

    public bool IsKnown { get; set; }

    public bool IsOnline { get; set; }

    public DeviceTransport? Transport { get; set; } = null!;

    public DeviceAdapter? Adapter { get; set; } = null!;
}