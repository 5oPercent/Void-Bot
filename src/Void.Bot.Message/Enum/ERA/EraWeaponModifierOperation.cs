namespace Void.Bot.Message.Enum.ERA;

/// <summary>武器改装修正方式；固定加值先累计，再应用累计百分比。</summary>
public enum EraWeaponModifierOperation
{
    /// <summary>固定加值；Value 表示对应属性单位的增减量。</summary>
    Flat = 1,
    /// <summary>加算百分比；Value 使用小数，例如 0.2 表示增加 20%。</summary>
    AddPercent = 2
}
