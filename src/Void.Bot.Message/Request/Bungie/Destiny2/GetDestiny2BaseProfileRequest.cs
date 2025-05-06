using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public class GetDestiny2BaseProfileRequest : IRequest
{
    [RegularExpression(BungieConstants.IdPattern)]
    public required string FindBungieName { get; set; }

    public BungieMembershipTypeEnum? MembershipType { get; set; }
}

public class GetDestiny2BaseProfileResponse : VoidBotResponse<Destiny2BaseProfileDto>
{
}

public class Destiny2BaseProfileDto
{
    public string DisplayName { get; set; } = string.Empty;
    
    public bool OnCrossSave { get; set; }
    
    public bool IsSinglePlatform { get; set; }
    
    public BungieMembershipTypeEnum MembershipTypeEnum { get; set; }
    
    public long MembershipId { get; set; }
}