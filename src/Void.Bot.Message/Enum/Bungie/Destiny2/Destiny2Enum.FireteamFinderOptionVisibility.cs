using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项可见性枚举
/// </summary>
public enum Destiny2FireteamFinderOptionVisibilityEnum
{
    /// <summary>
    /// 始终显示
    /// </summary>
    [Description("始终显示")]
    Always = 0,

    /// <summary>
    /// 仅在值与默认值不同时显示
    /// </summary>
    [Description("仅在值与默认值不同时显示")]
    ShowWhenChangedFromDefault = 1
}