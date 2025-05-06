using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public class GetDestiny2ProfileListByBungieNameRequest : IRequest
{
    public string DisplayName { get; set; }
    
    public int DisplayNameCode { get; set; }

    public BungieMembershipTypeEnum MembershipTypeEnum { get; set; } = BungieMembershipTypeEnum.All;
}

public class GetDestiny2ProfileListByBungieNameResponse : VoidBotResponse<Destiny2PlayerByBungieNameResponse>
{
}