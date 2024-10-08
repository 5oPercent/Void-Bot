using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Request.Identity;

public class LoginSteamOIDCRequest : IRequest
{
    public OIDCDto OpenID { get; set; }
}

public class LoginSteamRequest : LoginSteamOIDCRequest
{
    public AuthorizationDto Authorization { get; set; }
}

public class LoginSteamResponse : VoidBotResponse<string>
{
}