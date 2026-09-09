using HomeServer.Persistence.Interfeaces;

namespace HomeServer.Persistence.Events;

public sealed record DbEntityChangedEvent(
    IReadOnlyCollection<IDbEntity> Entities,
    DbEntityChangeType ChangeType);