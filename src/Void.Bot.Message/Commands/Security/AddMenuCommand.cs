using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class AddMenuCommand : ICommand
{
    public MenuDto Menu { get; set; }
}

public class AddMenuResponse : VoidBotResponse
{
}