using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HomeServer.Persistence.Interfeaces;

public interface IRepository<TEntity>
    where TEntity : class, IDbEntity
{
    Task<TResult> QueryAsync<TResult>(
        Func<DbSet<TEntity>, Task<TResult>> query,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken = default);
}