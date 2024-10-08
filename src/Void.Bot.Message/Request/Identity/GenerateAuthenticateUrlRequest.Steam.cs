using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Request.Identity;

public class GenerateAuthenticateUrlFromSteamRequest : IRequest
{
    public AuthorizationDto Authorization { get; set; }
}

public class GenerateAuthenticateUrlFromSteamResponse : VoidBotResponse<string>
{
}