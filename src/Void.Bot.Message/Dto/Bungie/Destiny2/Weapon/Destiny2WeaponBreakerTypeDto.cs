using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// 武器的固有反勇士属性。
/// </summary>
public class Destiny2WeaponBreakerTypeDto
{
    /// <summary>
    /// 反勇士属性类型。
    /// </summary>
    public Destiny2BreakerTypeEnum Type { get; set; }

    /// <summary>
    /// 破坏者类型定义哈希。
    /// </summary>
    public uint Hash { get; set; }

    /// <summary>
    /// 本地化名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 本地化描述。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Bungie 图标相对路径。
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}
