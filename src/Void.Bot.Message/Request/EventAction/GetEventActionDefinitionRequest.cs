using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.EventAction;

namespace Void.Bot.Message.Request.EventAction;

public class GetEventActionDefinitionRequest : IRequest
{
}

public class GetEventActionDefinitionResponse : VoidBotResponse<EventActionDefinitionData>
{
}
