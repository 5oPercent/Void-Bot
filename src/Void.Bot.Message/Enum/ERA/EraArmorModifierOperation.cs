namespace Void.Bot.Message.Enum.ERA;

/// <summary>护甲六维改装修正方式；固定加值先累计，再应用累计百分比。</summary>
public enum EraArmorModifierOperation
{
    /// <summary>固定加值；Value 为属性增减量，例如 5 表示增加 5 点。</summary>
    Flat = 1,
    /// <summary>加算百分比；Value 使用小数，例如 0.2 表示增加 20%，在固定加值累计后应用。</summary>
    AddPercent = 2
}
