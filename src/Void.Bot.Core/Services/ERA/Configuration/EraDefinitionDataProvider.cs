using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Configuration;

public interface IEraDefinitionDataProvider<TRecord> where TRecord : class, IEraDataDefinition
{
    Task<List<TRecord>> GetDefinitionsAsync(CancellationToken cancellationToken);
    Task<TRecord?> GetDefinitionAsync(string id, bool forUpdate, CancellationToken cancellationToken);
    Task AddAsync(TRecord record, CancellationToken cancellationToken);
    Task DeleteAsync(TRecord record, CancellationToken cancellationToken);
}

/// <summary>仅访问泛型参数对应的一张业务表，不提交事务或跨类别路由写入。</summary>
public abstract class EraDefinitionDataProvider<TRecord>(IRepository<TRecord> repository) : IEraDefinitionDataProvider<TRecord>
    where TRecord : class, IEraDataDefinition
{
    public Task<List<TRecord>> GetDefinitionsAsync(CancellationToken cancellationToken) =>
        repository.QueryNoTracking().ToListAsync(cancellationToken);

    public Task<TRecord?> GetDefinitionAsync(string id, bool forUpdate, CancellationToken cancellationToken) =>
        (forUpdate ? repository.Query() : repository.QueryNoTracking()).SingleOrDefaultAsync(record => record.StableId == id, cancellationToken);

    public Task AddAsync(TRecord record, CancellationToken cancellationToken) => repository.InsertAsync(record, cancellationToken);
    public Task DeleteAsync(TRecord record, CancellationToken cancellationToken) => repository.DeleteAsync(record, cancellationToken);
}
