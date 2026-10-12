namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 玩家在某个能力体系中最后使用的完整四槽配装。
/// </summary>
public sealed class AbilityLoadout
{
    /// <summary>
    /// 配装所属能力体系的稳定 ID。
    /// </summary>
    public string SystemId { get; set; } = string.Empty;

    /// <summary>
    /// 固定四槽技能 ID，顺序为特殊、主要、次要、战术；null 表示允许保留的空槽。
    /// </summary>
    public string?[] Slots { get; set; } = new string?[4];
}
