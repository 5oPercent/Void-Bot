using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项显示格式类型枚举
/// </summary>
public enum Destiny2FireteamFinderOptionDisplayFormatEnum
{
    /// <summary>
    /// 文本格式
    /// </summary>
    [Description("文本格式")]
    Text = 0,

    /// <summary>
    /// 整数格式
    /// </summary>
    [Description("整数格式")]
    Integer = 1,

    /// <summary>
    /// 布尔格式
    /// </summary>
    [Description("布尔格式")]
    Bool = 2,

    /// <summary>
    /// 格式化字符串
    /// </summary>
    [Description("格式化字符串")]
    FormatString = 3
}