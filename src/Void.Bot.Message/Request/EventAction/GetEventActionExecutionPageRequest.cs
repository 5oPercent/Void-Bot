using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.EventAction;

namespace Void.Bot.Message.Request.EventAction;

public class GetEventActionExecutionPageRequest : PageInputDto, IRequest
{
    public string? Event { get; set; }

    public Guid? RuleId { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
}

public class GetEventActionExecutionPageResponse : VoidBotResponse<PagedListDto<EventActionExecutionDto>>
{
}
