using System.Collections.Concurrent;
using HomeServer.Modules.Devices.Application.Interfaces;
using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Modules.Devices.Infrastructure.Services;

public sealed class RegistrationStore : IRegistrationStore
{
    private readonly ConcurrentDictionary<Guid, RegistrationContext> _registrations = [];

    public Guid Add(RegistrationContext context)
    {
        _registrations.TryAdd(context.Id, context);

        return context.Id;
    }

    public RegistrationContext? Get(Guid id)
    {
        _registrations.TryGetValue(id, out var context);

        return context;
    }

    public IReadOnlyCollection<RegistrationContext> GetAll()
    {
        return _registrations.Values.ToList();
    }

    public bool Remove(Guid id)
    {
        return _registrations.TryRemove(id, out _);
    }

    public bool TryGetByExternalId(string externalId, out RegistrationContext context)
    {
        context = _registrations.Values
            .FirstOrDefault(x =>
                x.AnalyzeResult?.Descriptor.ExternalId == externalId)!;

        return context is not null;
    }
}