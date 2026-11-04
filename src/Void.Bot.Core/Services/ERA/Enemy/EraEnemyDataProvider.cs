using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;

namespace Void.Bot.Core.Services.ERA.Enemy;

public interface IEraEnemyDataProvider : IEraDefinitionDataProvider<EraEnemyDefinition>, IScope
{
}

/// <summary>只访问本类别的当前定义表。</summary>
public sealed class EraEnemyDataProvider(IRepository<EraEnemyDefinition> repository)
    : EraDefinitionDataProvider<EraEnemyDefinition>(repository), IEraEnemyDataProvider
{
}
