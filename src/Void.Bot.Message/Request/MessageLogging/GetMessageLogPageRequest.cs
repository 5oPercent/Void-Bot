using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.MessageLogging;

namespace Void.Bot.Message.Request.MessageLogging;

public class GetMessageLogPageRequest : PageInputDto, IRequest
{
}

public class GetMessageLogPageResponse : VoidBotResponse<PagedListDto<MessageLogDto>>
{
}