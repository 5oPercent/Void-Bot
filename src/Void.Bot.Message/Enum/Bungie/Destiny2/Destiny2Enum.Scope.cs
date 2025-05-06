using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2范围枚举，定义数据或功能的应用范围层级
/// </summary>
public enum Destiny2ScopeEnum
{
    /// <summary>
    /// 个人资料
    /// </summary>
    [Description("个人资料")]
    Profile = 0,
    
    /// <summary>
    /// 角色
    /// </summary>
    [Description("角色")]
    Character = 1
}