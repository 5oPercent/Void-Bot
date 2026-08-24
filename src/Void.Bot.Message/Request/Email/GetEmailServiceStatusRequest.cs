using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Email;

namespace Void.Bot.Message.Request.Email;

public sealed class GetEmailServiceStatusRequest : IRequest
{
}

public sealed class GetEmailServiceStatusResponse : VoidBotResponse<EmailServiceStatusDto>
{
}
