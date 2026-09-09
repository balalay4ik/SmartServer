namespace HomeServer.Modules.Devices.Application.Models;

public class PendingCommand
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeviceId { get; set; }
    public string ExternalId { get; set; } = null!;
    public Guid CommandId { get; set; }
    public TransportRequest Request { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}