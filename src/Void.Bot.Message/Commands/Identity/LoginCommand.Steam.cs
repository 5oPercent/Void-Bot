using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Commands.Identity;

public class LoginSteamOIDCCommand : ICommand
{
    [JsonPropertyName("openid")]
    public required OpenIDConnectDto OpenId { get; set; }
}

public class LoginSteamCommand : LoginSteamOIDCCommand
{
    public required AuthorizationDto Authorization { get; set; }
}

public class LoginSteamResponse : VoidBotResponse<string>
{
}