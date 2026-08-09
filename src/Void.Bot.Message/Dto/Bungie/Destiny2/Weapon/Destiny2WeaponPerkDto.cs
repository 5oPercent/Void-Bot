namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// Localized logical perk and its manifest variants.
/// </summary>
public class Destiny2WeaponPerkDto
{
    public uint PerkHash { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int ApplicableWeaponCount { get; set; }

    public List<Destiny2WeaponPerkVariantDto> Variants { get; set; } = [];
}
