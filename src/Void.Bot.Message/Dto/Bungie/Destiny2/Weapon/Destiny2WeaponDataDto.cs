using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

public class Destiny2WeaponDataDto
{
    public uint Hash { get; set; }

    public Destiny2ItemSubTypeEnum WeaponType { get; set; }

    public string WeaponTypeName { get; set; } = string.Empty;

    public List<Destiny2WeaponPerkSocketDataDto> PerkSockets { get; set; } = [];
}
