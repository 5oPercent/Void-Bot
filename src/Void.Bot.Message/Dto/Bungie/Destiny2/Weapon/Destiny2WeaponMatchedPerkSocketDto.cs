namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// Weapon socket containing perks matched by a God Roll query.
/// </summary>
public class Destiny2WeaponMatchedPerkSocketDto
{
    public int SocketIndex { get; set; }

    public List<Destiny2WeaponMatchedPerkDto> Perks { get; set; } = [];
}
