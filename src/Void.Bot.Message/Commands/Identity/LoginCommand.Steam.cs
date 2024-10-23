using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Commands.Identity;

public class LoginSteamOIDCCommand : ICommand
{
    public required OIDCDto OpenID { get; set; }
}

public class LoginSteamCommand : LoginSteamOIDCCommand
{
    public required AuthorizationDto Authorization { get; set; }
}

public class LoginSteamResponse : VoidBotResponse<string>
{
}