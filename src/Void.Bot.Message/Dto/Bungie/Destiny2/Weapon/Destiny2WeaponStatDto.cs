namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// 武器面板属性。
/// </summary>
public class Destiny2WeaponStatDto
{
    /// <summary>
    /// 属性哈希。
    /// </summary>
    public uint StatHash { get; set; }

    /// <summary>
    /// 本地化属性名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 本地化属性描述。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 属性图标路径。
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 属性值。
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 属性最小值。
    /// </summary>
    public int Minimum { get; set; }

    /// <summary>
    /// 属性最大值。
    /// </summary>
    public int Maximum { get; set; }

    /// <summary>
    /// 面板显示最大值。
    /// </summary>
    public int DisplayMaximum { get; set; }
}
