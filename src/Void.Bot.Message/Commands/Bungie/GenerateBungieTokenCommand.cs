using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Request.Bungie.Domain;

namespace Void.Bot.Message.Commands.Bungie;

public class GenerateBungieTokenCommand : ICommand
{
    public required string OAuthCode { get; set; }
}

public class GenerateBungieTokenResponse : VoidBotResponse<BungieApiGenerateTokenResponse>
{
}