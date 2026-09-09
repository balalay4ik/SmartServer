using System.Dynamic;
using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public class CommandDescriptor
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }


    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public CommandControlType ControlType { get; set; }

    public string Method { get; set; } = null!;
    public string Endpoint { get; set; } = null!;

    public string ValueType { get; set; } = null!;
    public string? Value { get; set; } = null;

    public Dictionary<string, object>? Configuration { get; set; }
}