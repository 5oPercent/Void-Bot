namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillDraftSettings
{
    /// <summary>通用战术技能 ID，有序且不能重复。</summary>
    public List<string> CommonTacticalSkills { get; set; } = new();

    /// <summary>禁用技能 ID，不删除配装。</summary>
    public List<string> DisabledSkills { get; set; } = new();
}
