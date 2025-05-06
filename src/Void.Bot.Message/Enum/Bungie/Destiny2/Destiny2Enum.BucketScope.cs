using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运库位作用域枚举
/// </summary>
public enum Destiny2BucketScopeEnum
{
    /// <summary>
    /// 角色
    /// </summary>
    [Description("角色")]
    Character = 0,
    
    /// <summary>
    /// 账号
    /// </summary>
    [Description("账号")]
    Account = 1
}