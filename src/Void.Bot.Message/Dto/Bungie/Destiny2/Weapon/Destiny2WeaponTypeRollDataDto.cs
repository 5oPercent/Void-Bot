namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

public class Destiny2WeaponTypeRollDataDto
{
    public Destiny2WeaponTypeDataDto WeaponType { get; set; } = new();

    public Dictionary<uint, Destiny2WeaponDataDto> Weapons { get; set; } = [];

    public Dictionary<uint, Destiny2WeaponItemDataDto> Items { get; set; } = [];
}
