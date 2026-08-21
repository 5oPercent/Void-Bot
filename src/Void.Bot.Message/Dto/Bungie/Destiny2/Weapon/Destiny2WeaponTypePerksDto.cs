namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

public class Destiny2WeaponTypePerksDto
{
    public Destiny2WeaponTypeDto WeaponType { get; set; } = new();

    public List<Destiny2WeaponPerkSocketDto> PerkSockets { get; set; } = [];
}
