namespace HomeServer.Modules.Devices.Application.Models;

public class TransportRequest
{
    public string Address { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    public Dictionary<string, string> Metadata { get; set; }
        = [];
}