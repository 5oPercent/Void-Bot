namespace Void.Bot.Message.Dto.ERA;

/// <summary>技能编辑与发布管理数据，不参与局内运行模拟。</summary>
public sealed class EraSkillDraftDiagnostic
{
    /// <summary>稳定诊断代码。</summary>
    public string Code { get; set; } = "";

    /// <summary>具体对象或字段位置。</summary>
    public string Path { get; set; } = "";

    /// <summary>用于管理页面的诊断说明。</summary>
    public string Message { get; set; } = "";
}
