using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2插槽可见性枚举，定义插槽在界面中的显示状态
/// </summary>
public enum Destiny2SocketVisibilityEnum
{
    /// <summary>
    /// 可见，插槽始终在界面中显示
    /// </summary>
    [Description("可见")]
    Visible = 0,

    /// <summary>
    /// 隐藏，插槽在界面中不显示
    /// </summary>
    [Description("隐藏")]
    Hidden = 1,

    /// <summary>
    /// 空时隐藏，插槽为空时在界面中不显示
    /// </summary>
    [Description("空时隐藏")]
    HiddenWhenEmpty = 2,

    /// <summary>
    /// 无可用插件时隐藏，没有可插入插件时在界面中不显示
    /// </summary>
    [Description("无可用插件时隐藏")]
    HiddenIfNoPlugsAvailable = 3
}