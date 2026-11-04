using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;

namespace Void.Bot.Core.Services.ERA.Effect;

public interface IEraEffectDataProvider : IEraDefinitionDataProvider<EraEffectDefinition>, IScope
{
}

/// <summary>只访问本类别的当前定义表。</summary>
public sealed class EraEffectDataProvider(IRepository<EraEffectDefinition> repository)
    : EraDefinitionDataProvider<EraEffectDefinition>(repository), IEraEffectDataProvider
{
}
