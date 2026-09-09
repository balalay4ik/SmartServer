using System.Linq.Expressions;
using HomeServer.Persistence.Context;
using HomeServer.Persistence.Events;
using HomeServer.Persistence.Interfeaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HomeServer.Persistence.Repositories;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class, IDbEntity
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly Action<DbEntityChangedEvent> _onEntityChanged;

    public Repository(
        IDbContextFactory<AppDbContext> factory,
        Action<DbEntityChangedEvent> onEntityChanged
    )
    {
        _factory = factory;
        _onEntityChanged = onEntityChanged;
    }

    public async Task<TResult> QueryAsync<TResult>(
            Func<DbSet<TEntity>, Task<TResult>> query,
            CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        return await query(context.Set<TEntity>());
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        await context.Set<TEntity>().AddAsync(entity, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        _onEntityChanged(
        new DbEntityChangedEvent(
            new[] { entity },
            DbEntityChangeType.Added));
    }

    public async Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        context.Update(entity);

        await context.SaveChangesAsync(cancellationToken);

        _onEntityChanged(
        new DbEntityChangedEvent(
            new[] { entity },
            DbEntityChangeType.Updated));
    }

    public async Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        context.Remove(entity);

        await context.SaveChangesAsync(cancellationToken);

        _onEntityChanged(
        new DbEntityChangedEvent(
            new[] { entity },
            DbEntityChangeType.Removed));
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        var entity = await context.Set<TEntity>().FindAsync(
            [id],
            cancellationToken);

        if (entity is null)
            return;

        context.Remove(entity);

        await context.SaveChangesAsync(cancellationToken);

        _onEntityChanged(
        new DbEntityChangedEvent(
            new[] { entity },
            DbEntityChangeType.Removed));
    }

    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        await context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        _onEntityChanged(
        new DbEntityChangedEvent(
            entities.ToArray(),
            DbEntityChangeType.Added));
    }

    public async Task UpdateRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        context.Set<TEntity>().UpdateRange(entities);

        await context.SaveChangesAsync(cancellationToken);

        _onEntityChanged(
        new DbEntityChangedEvent(
            entities.ToArray(),
            DbEntityChangeType.Updated));
    }

    public async Task DeleteRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        context.Set<TEntity>().RemoveRange(entities);

        await context.SaveChangesAsync(cancellationToken);

        _onEntityChanged(
        new DbEntityChangedEvent(
            entities.ToArray(),
            DbEntityChangeType.Removed));
    }
}