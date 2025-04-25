using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2组件隐私设置枚举，定义API返回组件数据的隐私级别
/// </summary>
public enum Destiny2ComponentPrivacyEnum
{
    /// <summary>
    /// 无隐私保护，数据可以被任何请求访问
    /// </summary>
    [Description("无")]
    None = 0,
    
    /// <summary>
    /// 公开数据，可以被任何经过身份验证的请求访问
    /// </summary>
    [Description("公开")]
    Public = 1,
    
    /// <summary>
    /// 私有数据，只能被拥有者或具有特殊权限的请求访问
    /// </summary>
    [Description("私有")]
    Private = 2
}