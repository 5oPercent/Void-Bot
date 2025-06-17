using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public class GetDestiny2CareerRequest : IRequest
{
    public BungieMembershipTypeEnum MembershipType { get; set; }
    
    public long MembershipId { get; set; }
}

public class GetDestiny2CareerResponse : VoidBotResponse<Destiny2ProfileResponse>
{
}