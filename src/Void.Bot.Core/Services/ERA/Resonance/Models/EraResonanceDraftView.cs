using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance.Models;

/// <summary>一次一致性读取的编辑视图，不持久化第二份配置。</summary>
internal sealed class EraResonanceDraftView
{
    public EraResonanceDraftView(EraResonanceDraftWorkspace workspace, List<EraResonanceDraftRecord> records, EraResonanceConfiguration published)
    {
        Workspace = workspace;
        Records = records;
        Published = published;
    }

    public EraResonanceDraftWorkspace Workspace { get; }
    public List<EraResonanceDraftRecord> Records { get; }
    public EraResonanceConfiguration Published { get; }
}
