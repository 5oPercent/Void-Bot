using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetMenuTreeRequest : IRequest
{
}

public class GetMenuTreeResponse : VoidBotResponse<List<MenuDto>>
{
}