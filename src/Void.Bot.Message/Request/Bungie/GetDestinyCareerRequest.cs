using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie.Domain;

namespace Void.Bot.Message.Request.Bungie;

public class GetDestinyCareerRequest : IRequest
{
    public BungieMembershipType MembershipType { get; set; }
    
    public long MembershipId { get; set; }
}

public class GetDestinyCareerResponse : VoidBotResponse<BungieApiGetProfileFullResponse>
{
}