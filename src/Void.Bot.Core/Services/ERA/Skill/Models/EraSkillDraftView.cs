using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Skill.Models;

/// <summary>一次一致性读取的编辑视图，不持久化第二份配置。</summary>
internal sealed class EraSkillDraftView
{
    public EraSkillDraftView(EraSkillDraftWorkspace workspace, List<EraSkillDraftRecord> records, EraSkillConfiguration published)
    {
        Workspace = workspace;
        Records = records;
        Published = published;
    }

    public EraSkillDraftWorkspace Workspace { get; }
    public List<EraSkillDraftRecord> Records { get; }
    public EraSkillConfiguration Published { get; }
}
