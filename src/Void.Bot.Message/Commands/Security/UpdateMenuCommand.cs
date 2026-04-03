using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class UpdateMenuCommand : MenuBaseDto, ICommand
{
    public Guid Id { get; set; }
}

public class UpdateMenuResponse : VoidBotResponse
{
}