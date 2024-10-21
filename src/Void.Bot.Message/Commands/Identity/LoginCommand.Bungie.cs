using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Identity;

public class LoginBungieCommand : ICommand
{
    public string Code { get; set; }
    
    public string State { get; set; }
}

public class LoginBungieResponse : VoidBotResponse<string>
{
}