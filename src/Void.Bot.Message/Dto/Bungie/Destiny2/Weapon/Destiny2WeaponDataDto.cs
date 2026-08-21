using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

public class Destiny2WeaponDataDto
{
    public uint Hash { get; set; }

    public Destiny2ItemSubTypeEnum WeaponType { get; set; }

    public string WeaponTypeName { get; set; } = string.Empty;

    public Destiny2AmmunitionTypeEnum AmmoType { get; set; }

    public Destiny2DamageTypeEnum DamageType { get; set; }

    public Destiny2WeaponSlotEnum WeaponSlot { get; set; }

    public Destiny2TierTypeEnum Rarity { get; set; }

    public string RarityName { get; set; } = string.Empty;

    public Destiny2BreakerTypeEnum BreakerType { get; set; }

    public Destiny2WeaponBreakerTypeDto Breaker { get; set; } = new();

    public bool IsCraftable { get; set; }

    public uint RecipeItemHash { get; set; }

    public List<Destiny2WeaponStatDto> Stats { get; set; } = [];

    public List<Destiny2WeaponFoundryDto> Foundries { get; set; } = [];

    public Destiny2WeaponVersionDto Version { get; set; } = new();

    public bool IsAdept { get; set; }

    public bool IsHolofoil { get; set; }

    public Destiny2ClassEnum ClassType { get; set; }

    public uint FrameHash { get; set; }

    public int RoundsPerMinute { get; set; }

    public List<Destiny2WeaponPerkSocketDataDto> PerkSockets { get; set; } = [];
}
