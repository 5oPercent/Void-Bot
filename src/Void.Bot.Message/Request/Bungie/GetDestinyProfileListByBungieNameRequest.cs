using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie.Domain;

namespace Void.Bot.Message.Request.Bungie;

public class GetDestinyProfileListByBungieNameRequest : IRequest
{
    [Required]
    [RegularExpression(BungieConstants.IdPattern)]
    public string FindBungieName { get; set; }

    public BungieMembershipType MembershipType { get; set; } = BungieMembershipType.All;
}

public class GetDestinyProfileListByBungieNameResponse : VoidBotResponse<BungieApiSearchDestinyPlayerByBungieNameResponse>
{
}