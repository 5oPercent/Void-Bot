using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项控件类型枚举
/// </summary>
public enum Destiny2FireteamFinderOptionControlTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 值集合
    /// </summary>
    [Description("值集合")]
    ValueCollection = 1,

    /// <summary>
    /// 单选按钮
    /// </summary>
    [Description("单选按钮")]
    RadioButton = 2
}