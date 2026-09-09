using HomeServer.Persistence.Events;
using HomeServer.Persistence.Interfeaces;
using Microsoft.Extensions.DependencyInjection;

namespace HomeServer.Persistence.Repositories;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public event Func<DbEntityChangedEvent, Task>? EntityChanged;

    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IRepository<TEntity> Table<TEntity>()
        where TEntity : class, IDbEntity
    {
        return ActivatorUtilities.CreateInstance<Repository<TEntity>>(_serviceProvider, (Action<DbEntityChangedEvent>)OnEntityChanged);
    }

    private void OnEntityChanged(DbEntityChangedEvent @event)
    {
        EntityChanged?.Invoke(@event);
    }
}