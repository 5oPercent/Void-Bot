using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 记录值样式枚举，定义成就记录值的显示格式
/// </summary>
public enum Destiny2RecordValueStyleEnum
{
    /// <summary>
    /// 整数显示，以纯数字形式显示记录值
    /// </summary>
    [Description("整数")]
    Integer = 0,

    /// <summary>
    /// 百分比显示，以百分比形式显示记录值
    /// </summary>
    [Description("百分比")]
    Percentage = 1,

    /// <summary>
    /// 毫秒显示，以时间毫秒单位显示记录值
    /// </summary>
    [Description("毫秒")]
    Milliseconds = 2,

    /// <summary>
    /// 布尔值显示，以是/否形式显示记录值
    /// </summary>
    [Description("布尔值")]
    Boolean = 3,

    /// <summary>
    /// 小数显示，以带小数点形式显示记录值
    /// </summary>
    [Description("小数")]
    Decimal = 4
}