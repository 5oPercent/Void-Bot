namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// 武器面板属性过滤条件。
/// </summary>
public class Destiny2WeaponStatFilterDto
{
    /// <summary>
    /// 属性哈希。
    /// </summary>
    public uint StatHash { get; set; }

    /// <summary>
    /// 最小属性值，为空时不限制下限。
    /// </summary>
    public int? MinimumValue { get; set; }

    /// <summary>
    /// 最大属性值，为空时不限制上限。
    /// </summary>
    public int? MaximumValue { get; set; }
}
