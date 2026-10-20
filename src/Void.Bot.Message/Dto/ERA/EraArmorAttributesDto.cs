namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 单件护甲保存的最终六维整数。读取时不重新生成，不包含 BUFF，也不裁剪多件装备的溢出贡献。
/// </summary>
public sealed class EraArmorAttributesDto
{
    /// <summary>强度原值。</summary>
    public int Strength { get; init; }

    /// <summary>生存原值。</summary>
    public int Survival { get; init; }

    /// <summary>机动原值。</summary>
    public int Mobility { get; init; }

    /// <summary>聚焦原值；不表示本轮已接入具体效果。</summary>
    public int Focus { get; init; }

    /// <summary>效率原值；动态冷却由游戏运行时计算。</summary>
    public int Efficiency { get; init; }

    /// <summary>抗性原值；不表示本轮已接入具体效果。</summary>
    public int Resistance { get; init; }
}
