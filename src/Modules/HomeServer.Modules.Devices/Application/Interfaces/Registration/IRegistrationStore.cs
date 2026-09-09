
using HomeServer.Modules.Devices.Application.Models;

namespace HomeServer.Modules.Devices.Application.Interfaces;

public interface IRegistrationStore
{
    Guid Add(RegistrationContext context);

    RegistrationContext? Get(Guid id);

    IReadOnlyCollection<RegistrationContext> GetAll();

    bool Remove(Guid id);

    bool TryGetByExternalId(
        string externalId,
        out RegistrationContext context);
}