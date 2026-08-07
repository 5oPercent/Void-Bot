using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

/// <summary>
/// Filters weapons that satisfy every requested Perk Group on distinct sockets.
/// When no weapon type is supplied, all weapon types are searched.
/// </summary>
public class FilterDestiny2WeaponGodRollRequest : IRequest
{
    public List<Destiny2ItemSubTypeEnum> WeaponTypes { get; set; } = [];

    public List<Destiny2WeaponPerkGroupDto> PerkGroups { get; set; } = [];

    public string Language { get; set; } = string.Empty;
}

/// <summary>
/// God Roll weapon filter response.
/// </summary>
public class FilterDestiny2WeaponGodRollResponse : VoidBotResponse<Destiny2WeaponGodRollListDto>
{
}
