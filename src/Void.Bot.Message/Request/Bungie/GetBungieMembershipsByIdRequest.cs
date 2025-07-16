using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie;

namespace Void.Bot.Message.Request.Bungie;

public class GetBungieMembershipsByIdRequest : BungieApiMembershipsByIdRequest, IRequest
{
}

public class GetBungieMembershipsByIdResponse : VoidBotResponse<BungieApiMembershipsByIdResponse>
{
}