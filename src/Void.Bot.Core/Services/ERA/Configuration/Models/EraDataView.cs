using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Configuration.Models;

/// <summary>临时读取视图，不作为配置缓存持久化。</summary>
public sealed class EraDataView(EraDataEditState workspace, List<IEraDataDefinition> records)
{
    public EraDataEditState Workspace { get; } = workspace;
    public List<IEraDataDefinition> Records { get; } = records;
}
