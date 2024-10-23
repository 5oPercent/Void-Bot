using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Request.Identity;

public class GenerateAuthenticateUrlFromBungieRequest : IRequest
{
    public required AuthorizationDto Authorization { get; set; }
}

public class GenerateAuthenticateUrlFromBungieResponse : VoidBotResponse<string>
{
}