using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2插槽类型动作枚举，定义插入插件时可执行的不同动作类型
/// </summary>
public enum Destiny2SocketTypeActionTypeEnum
{
    /// <summary>
    /// 插入插件，将插件插入插槽
    /// </summary>
    [Description("插入插件")]
    InsertPlug = 0,

    /// <summary>
    /// 注入物品，将物品能量注入到目标物品
    /// </summary>
    [Description("注入物品")]
    InfuseItem = 1,

    /// <summary>
    /// 重置插槽，将插槽恢复到初始状态
    /// </summary>
    [Description("重置插槽")]
    ReinitializeSocket = 2
}