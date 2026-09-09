using HomeServer.Application.Models;
using HomeServer.Module.Devices.Application.Dto;
using HomeServer.Modules.Devices.Application.Dto;
using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Infrastructure.Extension;

public static class RegistrationMappingExtensions
{
    public static PendingRegistrationDto ToDto(
        this RegistrationContext context)
    {
        return new PendingRegistrationDto
        {
            Id = context.Id,

            Name = context.AnalyzeResult?.Descriptor.Name ?? "Unknown",

            Address = context.Address,

            Transport = context.Transport,

            Adapter = context.Adapter,

            Messages = context.AnalyzeResult?.Messages
            .Select(x => new PendingRegistrationMessageDto
            {
                Type = x.Type,
                Message = x.Message
            })
            .ToList() ?? [],

        };
    }

    public static IReadOnlyCollection<PendingRegistrationDto> ToDto(
        this IEnumerable<RegistrationContext> contexts)
    {
        return contexts
            .Select(x => x.ToDto())
            .ToList();
    }
}