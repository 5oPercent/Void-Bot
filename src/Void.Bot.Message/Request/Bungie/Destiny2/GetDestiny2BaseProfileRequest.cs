using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public class GetDestiny2BaseProfileRequest : IRequest
{
    public AppPlatformEnum? TriggerPlatform { get; set; }
    
    public string? TriggerPlatformUserId { get; set; }

    public string? FindBungieName { get; set; }

    public BungieMembershipTypeEnum? MembershipType { get; set; }
}

public class GetDestiny2BaseProfileResponse : VoidBotResponse<Destiny2BaseProfileDto>
{
}

public class Destiny2BaseProfileDto
{
    public bool OnCrossSave { get; set; }
    
    public bool IsSinglePlatform { get; set; }
    
    public BungieMembershipTypeEnum MembershipTypeEnum { get; set; }
    
    public long MembershipId { get; set; }
}