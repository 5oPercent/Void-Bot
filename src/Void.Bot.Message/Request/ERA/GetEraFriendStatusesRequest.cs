using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraFriendStatusesRequest : IRequest
{
    public List<Guid> AccountIds { get; set; } = new List<Guid>();
}

public class GetEraFriendStatusesResponse : VoidBotResponse<List<EraFriendStatusDto>>
{
}
