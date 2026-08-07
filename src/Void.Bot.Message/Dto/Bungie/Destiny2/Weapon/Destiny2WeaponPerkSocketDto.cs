namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// Perks available in a weapon socket.
/// </summary>
public class Destiny2WeaponPerkSocketDto
{
    public int SocketIndex { get; set; }

    public List<Destiny2WeaponPerkDto> Perks { get; set; } = [];
}
