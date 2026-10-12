using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 一项 BUFF 效果，属性修正和即时／周期结算按效果类型解释对应参数。
/// </summary>
public sealed class BuffEffectDto
{
    /// <summary>
    /// 当前定义的类型，决定如何解释效果或 BUFF 生命周期。
    /// </summary>
    public BuffEffectKind Kind { get; set; }

    /// <summary>
    /// 效果数值；属性修正按运算类型解释，伤害／治疗／防护恢复为非负实际数值。
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// 属性修正目标协议值：0 力量、1 体质、2 敏捷、3 武器攻击、4 最大生命、5 移速、6 最大防护。
    /// </summary>
    public int Stat { get; set; }

    /// <summary>
    /// 属性修正运算协议值：0 固定加值、1 加算百分比、2 独立倍率。
    /// </summary>
    public int Operation { get; set; }
}
