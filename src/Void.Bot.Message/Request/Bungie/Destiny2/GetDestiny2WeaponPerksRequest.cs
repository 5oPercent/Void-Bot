using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

/// <summary>
/// Requests possible perks for one or more weapon types.
/// </summary>
public class GetDestiny2WeaponPerksRequest : IRequest
{
    public List<Destiny2ItemSubTypeEnum> WeaponTypes { get; set; } = [];

    public string Language { get; set; } = string.Empty;

    public string? Keyword { get; set; }
}

/// <summary>
/// Weapon-type-scoped perk query response.
/// </summary>
public class GetDestiny2WeaponPerksResponse : VoidBotResponse<Destiny2WeaponPerksDto>
{
}
