using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2目标UI样式枚举，定义目标在界面中的显示样式
/// </summary>
public enum Destiny2ObjectiveUiStyleEnum
{
    /// <summary>
    /// 无样式，使用默认显示
    /// </summary>
    [Description("无样式")]
    None = 0,

    /// <summary>
    /// 高亮显示样式
    /// </summary>
    [Description("高亮")]
    Highlighted = 1,

    /// <summary>
    /// 制造武器等级样式
    /// </summary>
    [Description("武器等级")]
    CraftingWeaponLevel = 2,

    /// <summary>
    /// 制造武器等级进度样式
    /// </summary>
    [Description("武器进度")]
    CraftingWeaponLevelProgress = 3,

    /// <summary>
    /// 制造武器时间戳样式
    /// </summary>
    [Description("武器时间戳")]
    CraftingWeaponTimestamp = 4,

    /// <summary>
    /// 制造纪念品样式
    /// </summary>
    [Description("制造纪念品")]
    CraftingMementos = 5,

    /// <summary>
    /// 制造纪念品标题样式
    /// </summary>
    [Description("纪念品标题")]
    CraftingMementoTitle = 6
}