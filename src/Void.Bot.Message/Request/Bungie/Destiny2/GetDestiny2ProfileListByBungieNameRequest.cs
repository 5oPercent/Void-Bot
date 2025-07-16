using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public class GetDestiny2ProfileListByBungieNameRequest : IRequest, IRequestDestiny2ByPlayerNameCode
{
    [MinLength(1)]
    public required string DisplayName { get; set; }
    
    [Length(4, 4)]
    public required string DisplayNameCode { get; set; }

    public BungieMembershipTypeEnum MembershipType { get; set; } = BungieMembershipTypeEnum.All;
}

public class GetDestiny2ProfileListByBungieNameResponse : VoidBotResponse<Destiny2PlayerByBungieNameResponse>
{
}