using HomeServer.Modules.Devices.Domain.Enums;

namespace HomeServer.Modules.Devices.Application.Dto;

public sealed class PendingRegistrationMessageDto
{
    public AdapterMessageType Type { get; set; }

    public string Message { get; set; } = string.Empty;
}