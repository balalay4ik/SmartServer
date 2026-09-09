using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Models;

public sealed class AdapterMessage
{
    public required AdapterMessageType Type { get; init; }

    public required string Message { get; init; }
}