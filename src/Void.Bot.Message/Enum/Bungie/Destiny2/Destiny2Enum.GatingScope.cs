using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2权限作用域范围枚举
/// 定义了游戏中各种能力、物品或功能的可见性和可访问性的作用域范围
/// </summary>
public enum Destiny2GatingScopeEnum
{
    /// <summary>
    /// 无作用域
    /// </summary>
    [Description("无作用域")]
    None = 0,
    
    /// <summary>
    /// 全局作用域，适用于所有玩家
    /// </summary>
    [Description("全局作用域")]
    Global = 1,
    
    /// <summary>
    /// 公会作用域，限制在特定公会内
    /// </summary>
    [Description("公会作用域")]
    Clan = 2,
    
    /// <summary>
    /// 个人档案作用域，限制在玩家的账户范围内
    /// </summary>
    [Description("个人档案作用域")]
    Profile = 3,
    
    /// <summary>
    /// 角色作用域，限制在玩家的特定角色范围内
    /// </summary>
    [Description("角色作用域")]
    Character = 4,
    
    /// <summary>
    /// 物品作用域，限制在特定物品上
    /// </summary>
    [Description("物品作用域")]
    Item = 5,
    
    /// <summary>
    /// 假定最差情况，用于权限检查的保守处理
    /// </summary>
    [Description("假定最差情况")]
    AssumedWorstCase = 6
}