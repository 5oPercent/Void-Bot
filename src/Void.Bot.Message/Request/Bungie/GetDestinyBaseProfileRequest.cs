using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie;

public class GetDestinyBaseProfileRequest : IRequest
{
    [RegularExpression(BungieConstants.IdPattern)]
    public string? FindBungieName { get; set; }

    public BungieMembershipType? MembershipType { get; set; }
}

public class GetDestinyBaseProfileResponse : VoidBotResponse<DestinyBaseProfileDto>
{
}

public class DestinyBaseProfileDto
{
    public bool OnCrossSave { get; set; }
    
    public bool IsSinglePlatform { get; set; }
    
    public BungieMembershipType MembershipType { get; set; }
    
    public long MembershipId { get; set; }
}