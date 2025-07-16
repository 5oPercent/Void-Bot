using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

[LoadDestiny2Player]
public class GetDestiny2BaseProfileRequest : IRequest, IRequestDestiny2ByPlayerNameCode
{
    public string DisplayName { get; set; } = string.Empty;
    
    public string DisplayNameCode { get; set; } = string.Empty;

    public BungieMembershipTypeEnum MembershipType { get; set; }
}

public class GetDestiny2BaseProfileResponse : VoidBotResponse<Destiny2BaseProfileDto>
{
}

public class Destiny2BaseProfileDto
{
    public string DisplayName { get; set; } = string.Empty;
    
    public bool OnCrossSave { get; set; }
    
    public bool IsSinglePlatform { get; set; }
    
    public BungieMembershipTypeEnum MembershipType { get; set; }
    
    public string MembershipId { get; set; } = string.Empty;
}