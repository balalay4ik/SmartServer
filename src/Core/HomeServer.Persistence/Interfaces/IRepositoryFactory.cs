using HomeServer.Persistence.Events;

namespace HomeServer.Persistence.Interfeaces;

public interface IRepositoryFactory
{
    event Func<DbEntityChangedEvent, Task>? EntityChanged;
    IRepository<TEntity> Table<TEntity>()
        where TEntity : class, IDbEntity;
}