using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Message.Request.Social;

public class GetFriendsRequest : IRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetFriendsResponse : VoidBotResponse<SocialPageDto<FriendDto>>
{
}
