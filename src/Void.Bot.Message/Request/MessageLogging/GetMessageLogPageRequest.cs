using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.MessageLogging;

namespace Void.Bot.Message.Request.MessageLogging;

public class GetMessageLogPageRequest : PageInputDto, IRequest
{
    public List<Guid>? Ids { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
}

public class GetMessageLogPageResponse : VoidBotResponse<PagedListDto<MessageLogDto>>
{
}