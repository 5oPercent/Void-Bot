namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// Localized perk matched by a God Roll query.
/// </summary>
public class Destiny2WeaponMatchedPerkDto
{
    public uint PerkHash { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public bool IsEnhanced { get; set; }
}
