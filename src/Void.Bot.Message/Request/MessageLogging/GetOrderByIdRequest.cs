using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.MessageLogging;

namespace Void.Bot.Message.Request.MessageLogging;

public class GetMessageLogRequest : IRequest
{
    public List<Guid>? Ids { get; set; }

    public DateTimeOffset? startDate { get; set; }

    public DateTimeOffset? endDate { get; set; }
}

public class GetMessageLogResponse : VoidBotResponse<List<MessageLogDto>>
{
}