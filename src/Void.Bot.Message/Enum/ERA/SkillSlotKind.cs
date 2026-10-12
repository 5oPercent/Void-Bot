namespace Void.Bot.Message.Enum.ERA;

/// <summary>
/// 技能组的四个固定槽位；数值用于存档与网络协议，不得重排。
/// </summary>
public enum SkillSlotKind
{
    /// <summary>
    /// 特殊技能槽，通常用于低频强力技能。
    /// </summary>
    Special = 0,

    /// <summary>
    /// 主要技能槽，选择技能的主要释放方案。
    /// </summary>
    Primary = 1,

    /// <summary>
    /// 次要技能槽，选择技能的次要释放方案。
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// 战术技能槽，可选择体系专属和通用战术技能。
    /// </summary>
    Tactical = 3,
}
