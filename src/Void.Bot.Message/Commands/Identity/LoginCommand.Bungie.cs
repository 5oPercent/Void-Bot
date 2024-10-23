using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Identity;

public class LoginBungieCommand : ICommand
{
    public required string Code { get; set; }
    
    public required string State { get; set; }
}

public class LoginBungieResponse : VoidBotResponse<string>
{
}