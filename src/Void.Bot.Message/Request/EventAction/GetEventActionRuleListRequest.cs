using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.EventAction;

namespace Void.Bot.Message.Request.EventAction;

public class GetEventActionRuleListRequest : IRequest
{
    public string? Event { get; set; }
}

public class GetEventActionRuleListResponse : VoidBotResponse<List<EventActionRuleDto>>
{
}
