using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Void.Bot.Core.Services;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Data;

/// <summary>无作用域数据库上下文依赖的跨存储协调入口；普通实体访问使用 IRepository&lt;TEntity&gt;。</summary>
public interface IRepository : ISingleton
{
    Task<T> ExecuteInIndependentTransactionAsync<T>(IEnumerable<FormattableString> statements,
        Func<Task<T>> action, CancellationToken cancellationToken);
}

public interface IRepository<TEntity>  where TEntity : class, IEntityBase
{
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<List<TEntity>> SqlQueryAsync(string sql, params object[] parameters);

    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null);

    IQueryable<TEntity> QueryNoTracking(Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>使用参数化 SQL 读取实体；查询跟踪行为与 Query 相同。</summary>
    IQueryable<TEntity> QuerySql(FormattableString sql);

    /// <summary>读取不含动态参数的固定 SQL；动态值使用 QuerySql。</summary>
    IQueryable<TEntity> QuerySqlRaw(string sql);

    /// <summary>写入前校验外层工作单元已开启事务，不创建或提交事务。</summary>
    void RequireTransaction();

    Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default);

    /// <summary>在外层事务中立即执行条件更新，不提交事务。</summary>
    Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setters,
        CancellationToken cancellationToken = default);

    /// <summary>在外层事务中立即执行条件删除，不提交事务。</summary>
    Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    void Insert(TEntity entity);

    void InsertMany(IEnumerable<TEntity> entities);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    void DeleteMany(IEnumerable<TEntity> entities);
}
