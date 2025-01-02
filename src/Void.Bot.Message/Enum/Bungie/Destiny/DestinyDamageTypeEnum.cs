using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny;

/// <summary>
/// 命运伤害类型枚举
/// </summary>
public enum DestinyDamageTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,
    
    /// <summary>
    /// 动能
    /// </summary>
    [Description("动能")]
    Kinetic = 1,
    
    /// <summary>
    /// 电弧
    /// </summary>
    [Description("电弧")]
    Arc = 2,
    
    /// <summary>
    /// 灼烧
    /// </summary>
    [Description("灼烧")]
    Thermal = 3,
    
    /// <summary>
    /// 虚空
    /// </summary>
    [Description("虚空")]
    Void = 4,
    
    /// <summary>
    /// 突袭
    /// </summary>
    [Description("突袭")]
    Raid = 5,
    
    /// <summary>
    /// 冰影
    /// </summary>
    [Description("冰影")]
    Stasis = 6,
    
    /// <summary>
    /// 缚丝
    /// </summary>
    [Description("缚丝")]
    Strand = 7
}