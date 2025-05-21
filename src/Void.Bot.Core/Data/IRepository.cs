using System.Linq.Expressions;
using Void.Bot.Core.Domain;

namespace Void.Bot.Core.Data;

public interface IRepository<TEntity>  where TEntity : class, IEntityBase
{
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task InsertAllAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAllAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAllAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TEntity>> SqlQueryAsync(string sql, params object[] parameters);

    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null);

    IQueryable<TEntity> QueryNoTracking(Expression<Func<TEntity, bool>>? predicate = null);
}