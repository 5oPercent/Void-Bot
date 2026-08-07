using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

public class Destiny2WeaponRollDataDto
{
    public Dictionary<uint, Destiny2WeaponItemDataDto> Items { get; set; } = [];

    public Dictionary<uint, Destiny2WeaponDataDto> Weapons { get; set; } = [];

    public Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto> WeaponTypes { get; set; } = [];

    public Dictionary<uint, HashSet<uint>> PerkVariantGroups { get; set; } = [];
}
