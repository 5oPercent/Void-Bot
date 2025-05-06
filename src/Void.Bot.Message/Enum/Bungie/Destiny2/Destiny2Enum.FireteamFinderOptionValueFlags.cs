using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项值标志位枚举
/// </summary>
public enum Destiny2FireteamFinderOptionValueFlagEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,

    /// <summary>
    /// 创建清单的默认值
    /// </summary>
    [Description("创建清单的默认值")]
    CreateListingDefaultValue = 1,

    /// <summary>
    /// 搜索过滤的默认值
    /// </summary>
    [Description("搜索过滤的默认值")]
    SearchFilterDefaultValue = 2
}