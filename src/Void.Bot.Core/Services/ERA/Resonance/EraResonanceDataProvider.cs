using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;

namespace Void.Bot.Core.Services.ERA.Resonance;

public interface IEraResonanceDataProvider : IEraDefinitionDataProvider<EraSkillDefinition>, IScope
{
}

/// <summary>只访问本类别的当前定义表。</summary>
public sealed class EraResonanceDataProvider(IRepository<EraSkillDefinition> repository)
    : EraDefinitionDataProvider<EraSkillDefinition>(repository), IEraResonanceDataProvider
{
}
