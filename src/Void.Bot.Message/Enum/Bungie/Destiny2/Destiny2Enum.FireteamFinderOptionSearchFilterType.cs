using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 组队查找器选项搜索过滤类型枚举
/// </summary>
public enum Destiny2FireteamFinderOptionSearchFilterTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    [Description("无")]
    None = 0,
    
    /// <summary>
    /// 全部
    /// </summary>
    [Description("全部")]
    All = 1,
    
    /// <summary>
    /// 任意
    /// </summary>
    [Description("任意")]
    Any = 2,
    
    /// <summary>
    /// 在范围内（包含边界）
    /// </summary>
    [Description("在范围内（包含边界）")]
    InRangeInclusive = 3,
    
    /// <summary>
    /// 在范围内（不包含边界）
    /// </summary>
    [Description("在范围内（不包含边界）")]
    InRangeExclusive = 4,
    
    /// <summary>
    /// 大于
    /// </summary>
    [Description("大于")]
    GreaterThan = 5,
    
    /// <summary>
    /// 大于等于
    /// </summary>
    [Description("大于等于")]
    GreaterThanOrEqualTo = 6,
    
    /// <summary>
    /// 小于
    /// </summary>
    [Description("小于")]
    LessThan = 7,
    
    /// <summary>
    /// 小于等于
    /// </summary>
    [Description("小于等于")]
    LessThanOrEqualTo = 8
}