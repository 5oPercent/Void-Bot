using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Request.Account;

public sealed class GetUserEmailStatusRequest : IRequest
{
}

public sealed class GetUserEmailStatusResponse : VoidBotResponse<UserEmailStatusDto>
{
}
