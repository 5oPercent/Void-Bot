using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2商人显示类别排序顺序枚举，定义商人界面中显示类别的排序方式
/// </summary>
public enum Destiny2VendorDisplayCategorySortOrderEnum
{
    /// <summary>
    /// 默认排序方式，按游戏预设顺序排列
    /// </summary>
    [Description("默认")]
    Default = 0,

    /// <summary>
    /// 按品质层级排序，根据物品的稀有度或品质等级进行排序
    /// </summary>
    [Description("按品质排序")]
    SortByTier = 1
}