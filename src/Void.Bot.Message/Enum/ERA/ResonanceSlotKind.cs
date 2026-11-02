namespace Void.Bot.Message.Enum.ERA;

/// <summary>
/// 谐振技能组的四个固定槽位；数值用于存档与网络协议，不得重排。
/// </summary>
public enum ResonanceSlotKind
{
    /// <summary>
    /// 特殊谐振技能槽，通常用于低频强力谐振技能。
    /// </summary>
    Special = 0,

    /// <summary>
    /// 主要谐振技能槽，选择谐振技能的主要释放方案。
    /// </summary>
    Primary = 1,

    /// <summary>
    /// 次要谐振技能槽，选择谐振技能的次要释放方案。
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// 战术谐振技能槽，可选择晶格体系专属和通用战术谐振技能。
    /// </summary>
    Tactical = 3,
}
