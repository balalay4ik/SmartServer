using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public sealed class RegistrationCompleteRequest
{
    public string Name { get; init; } = null!;
    public DeviceAdapter? Adapter { get; init; }
}