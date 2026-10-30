namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器改装修正方式；有限属性值先累计固定加值，再应用累计百分比，效果端点不参与修正。</summary>
public enum EraWeaponModifierOperation
{
    /// <summary>固定加值：Value 使用目标属性的单位或评分点，例如伤害 +5、操控 +10。</summary>
    Flat = 1,
    /// <summary>加算百分比：Value 使用小数且必须大于 -1，例如 0.2 表示增加 20%；多项百分比先相加。</summary>
    AddPercent = 2
}
