using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

[LoadDestiny2Player]
public class GetDestiny2CareerRequest : IRequest, IRequestDestiny2ByPlayerNameCode
{
    public string DisplayName { get; set; } = string.Empty;
    
    public string DisplayNameCode { get; set; } = string.Empty;
    
    public BungieMembershipTypeEnum MembershipType { get; set; }
}

public class GetDestiny2CareerResponse : VoidBotResponse<Destiny2ProfileResponse>
{
}