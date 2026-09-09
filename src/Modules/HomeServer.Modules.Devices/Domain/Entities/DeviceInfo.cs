using HomeServer.Persistence.Interfeaces;

namespace HomeServer.Modules.Devices.Domain.Entities;

public class DeviceInfo : IDbEntity
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Device Device { get; set; } = null!;

    // Производитель
    public string? Vendor { get; set; }

    public string? Model { get; set; }

    public string? Product { get; set; }

    public string? HardwareVersion { get; set; }

    public string? FirmwareVersion { get; set; }

    public string? ProtocolVersion { get; set; }

    // Идентификаторы
    public string? SerialNumber { get; set; }

    public string? MacAddress { get; set; }

    // Дополнительная информация
    public string? BuildDate { get; set; }

    public string? Description { get; set; }

    public string? AdditionalDataJson { get; set; }

    public string? ConfigurationJson { get; set; }
}