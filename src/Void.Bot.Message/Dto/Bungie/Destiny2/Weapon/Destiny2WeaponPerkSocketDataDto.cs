namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

public class Destiny2WeaponPerkSocketDataDto
{
    public int SocketIndex { get; set; }

    public HashSet<uint> PerkHashes { get; set; } = [];
}
