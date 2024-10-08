using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Request.Identity;

public class LoginBungieRequest : IRequest
{
    public string Code { get; set; }
    
    public string State { get; set; }
}

public class LoginBungieResponse : VoidBotResponse<string>
{
}