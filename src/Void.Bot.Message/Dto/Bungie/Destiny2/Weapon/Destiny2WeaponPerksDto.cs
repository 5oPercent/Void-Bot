using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// Weapon-type-scoped perk list payload.
/// </summary>
public class Destiny2WeaponPerksDto
{
    public List<Destiny2WeaponTypePerksDto> WeaponTypes { get; set; } = [];

    public List<Destiny2ItemSubTypeEnum> UnmatchedWeaponTypes { get; set; } = [];
}
