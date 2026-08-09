namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// Weapon matching all requested Perk Groups.
/// </summary>
public class Destiny2WeaponGodRollDto
{
    public uint WeaponHash { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public Destiny2WeaponTypeReferenceDto WeaponType { get; set; } = new();

    public Destiny2WeaponFrameReferenceDto Frame { get; set; } = new();

    public int RoundsPerMinute { get; set; }

    public List<Destiny2WeaponMatchedPerkSocketDto> MatchedPerkSockets { get; set; } = [];
}
