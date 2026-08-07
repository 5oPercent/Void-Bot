using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

public class Destiny2WeaponTypeDataDto
{
    public Destiny2ItemSubTypeEnum WeaponType { get; set; }

    public string Name { get; set; } = string.Empty;

    public HashSet<uint> WeaponHashes { get; set; } = [];
}
