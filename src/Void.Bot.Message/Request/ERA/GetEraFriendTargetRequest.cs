using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraFriendTargetRequest : IRequest, IRequireEraPlayer
{
    public Guid FriendAccountId { get; set; }
}

public class GetEraFriendTargetResponse : VoidBotResponse<EraFriendStatusDto>
{
}
