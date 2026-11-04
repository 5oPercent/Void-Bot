using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;

namespace Void.Bot.Core.Services.ERA.Lattice;

public interface IEraLatticeDataProvider : IEraDefinitionDataProvider<EraLatticeDefinition>, IScope
{
}

/// <summary>只访问本类别的当前定义表。</summary>
public sealed class EraLatticeDataProvider(IRepository<EraLatticeDefinition> repository)
    : EraDefinitionDataProvider<EraLatticeDefinition>(repository), IEraLatticeDataProvider
{
}
