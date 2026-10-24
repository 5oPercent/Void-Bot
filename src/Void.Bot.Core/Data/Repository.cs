using System.Linq.Expressions;
using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MySqlConnector;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Data;

/// <summary>独立协调事务仅保护跨存储提交，不复用或提交调用方的资产工作单元。</summary>
public class Repository(ConnectionStringSetting setting) : IRepository
{
    public async Task<T> ExecuteInIndependentTransactionAsync<T>(IEnumerable<FormattableString> statements,
        Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(setting.Mysql);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken).ConfigureAwait(false);
        foreach (var statement in statements)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            var names = new object[statement.ArgumentCount];
            for (var index = 0; index < statement.ArgumentCount; index++)
            {
                var name = "@p" + index.ToString(CultureInfo.InvariantCulture);
                names[index] = name;
                command.Parameters.AddWithValue(name, statement.GetArgument(index) ?? DBNull.Value);
            }
            command.CommandText = string.Format(CultureInfo.InvariantCulture, statement.Format, names);
            await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        }
        var result = await action().ConfigureAwait(false);
        // 外部存储已收到提交后等待 SQL 完成；取消不能把已执行的外部写入误判成未提交。
        await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
        return result;
    }
}

public class Repository<TEntity>(VoidBotDbContext dbContext) : IRepository<TEntity> where TEntity : class, IEntityBase
{
    public async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await dbContext.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        dbContext.Update(entity);
        return Task.CompletedTask;
    }

    public Task UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        dbContext.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        dbContext.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        dbContext.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (entities.Count != 0)
        {
            dbContext.RemoveRange(entities);
        }
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().CountAsync(predicate, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<TEntity>> SqlQueryAsync(string sql, params object[] parameters)
    {
        return await dbContext.Set<TEntity>().FromSqlRaw(sql, parameters).ToListAsync().ConfigureAwait(false);
    }

    public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null ? dbContext.Set<TEntity>() : dbContext.Set<TEntity>().Where(predicate);
    }

    public IQueryable<TEntity> QueryNoTracking(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null
            ? dbContext.Set<TEntity>().AsNoTracking()
            : dbContext.Set<TEntity>().AsNoTracking().Where(predicate);
    }

    public IQueryable<TEntity> QuerySql(FormattableString sql)
    {
        return dbContext.Set<TEntity>().FromSqlInterpolated(sql);
    }

    public IQueryable<TEntity> QuerySqlRaw(string sql)
    {
        return dbContext.Set<TEntity>().FromSqlRaw(sql);
    }

    public void RequireTransaction()
    {
        if (dbContext.Database.CurrentTransaction == null)
            throw new InvalidOperationException("This operation requires an outer unit-of-work transaction.");
    }

    public Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default)
    {
        RequireTransaction();
        return dbContext.Database.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
    }

    public Task<int> ExecuteUpdateAsync(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setters,
        CancellationToken cancellationToken = default)
    {
        RequireTransaction();
        return dbContext.Set<TEntity>().Where(predicate).ExecuteUpdateAsync(setters, cancellationToken);
    }

    public Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        RequireTransaction();
        return dbContext.Set<TEntity>().Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }

    public void Insert(TEntity entity) => dbContext.Set<TEntity>().Add(entity);

    public void InsertMany(IEnumerable<TEntity> entities) => dbContext.Set<TEntity>().AddRange(entities);

    public void Update(TEntity entity) => dbContext.Set<TEntity>().Update(entity);

    public void Delete(TEntity entity) => dbContext.Set<TEntity>().Remove(entity);

    public void DeleteMany(IEnumerable<TEntity> entities) => dbContext.Set<TEntity>().RemoveRange(entities);
}
