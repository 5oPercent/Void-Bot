using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// Weapon matching all requested Perk Groups.
/// </summary>
public class Destiny2WeaponGodRollDto
{
    public uint WeaponHash { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public Destiny2WeaponAppearanceDto Appearance { get; set; } = new();

    public Destiny2WeaponTypeReferenceDto WeaponType { get; set; } = new();

    public Destiny2AmmunitionTypeEnum AmmoType { get; set; }

    public Destiny2DamageTypeEnum DamageType { get; set; }

    public Destiny2WeaponSlotEnum WeaponSlot { get; set; }

    public Destiny2TierTypeEnum Rarity { get; set; }

    public string RarityName { get; set; } = string.Empty;

    public Destiny2WeaponBreakerTypeDto Breaker { get; set; } = new();

    public bool IsCraftable { get; set; }

    public uint RecipeItemHash { get; set; }

    public List<Destiny2WeaponStatDto> Stats { get; set; } = [];

    public List<Destiny2WeaponFoundryDto> Foundries { get; set; } = [];

    public Destiny2WeaponVersionDto Version { get; set; } = new();

    public bool IsAdept { get; set; }

    public bool IsHolofoil { get; set; }

    public Destiny2ClassEnum ClassType { get; set; }

    public Destiny2WeaponFrameReferenceDto Frame { get; set; } = new();

    public int RoundsPerMinute { get; set; }

    public Destiny2WeaponSeasonReferenceDto Season { get; set; } = new();

    public List<Destiny2WeaponMatchedPerkSocketDto> MatchedPerkSockets { get; set; } = [];
}
