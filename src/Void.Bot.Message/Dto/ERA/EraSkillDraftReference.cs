namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillDraftReference
{
    /// <summary>引用类别。</summary>
    public string Type { get; set; } = "";

    /// <summary>引用对象 ID。</summary>
    public string Id { get; set; } = "";

    /// <summary>引用位置。</summary>
    public string Path { get; set; } = "";
}
