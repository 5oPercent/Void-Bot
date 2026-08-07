using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie;

namespace Void.Bot.Message.Request.Bungie;

public class GetBungieNetUserRequest : BungieApiNetUserRequest, IRequest
{
}

public class GetBungieNetUserResponse : VoidBotResponse<BungieApiNetUserResponse>
{
}