namespace HomeServer.Modules.Devices.Application.Models;

public class DeviceRegistrationContext
{
    public required DiscoveredDevice Discovered { get; init; }
    public required DeviceDescriptor Descriptor { get; init; }
}