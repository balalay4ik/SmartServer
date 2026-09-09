using System.ComponentModel.DataAnnotations.Schema;
using HomeServer.Modules.Devices.Domain.Enums;
using HomeServer.Persistence.Interfeaces;

namespace HomeServer.Modules.Devices.Domain.Entities;

public class Command : IDbEntity
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Device Device { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public string? Method { get; set; } = null!;

    public CommandControlType ControlType { get; set; }

    public string ValueType { get; set; } = null!;

    public string? ConfigurationJson { get; set; }
}