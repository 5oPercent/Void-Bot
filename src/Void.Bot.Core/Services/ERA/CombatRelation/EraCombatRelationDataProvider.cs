using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.CombatRelation;

public interface IEraCombatRelationDataProvider : IScope
{
    Task<EraCombatRelationConfiguration?> GetAsync(bool forUpdate, CancellationToken cancellationToken);
}

/// <summary>种族、阵营及默认关系的独立业务表访问。</summary>
public sealed class EraCombatRelationDataProvider(IRepository<EraCombatRelationConfiguration> repository) : IEraCombatRelationDataProvider
{
    public Task<EraCombatRelationConfiguration?> GetAsync(bool forUpdate, CancellationToken cancellationToken) =>
        (forUpdate ? repository.Query() : repository.QueryNoTracking()).SingleOrDefaultAsync(cancellationToken);
}
