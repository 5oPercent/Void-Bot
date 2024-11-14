using Mediator.Net.Contracts;
using Void.Bot.Message.Commands.Bungie.Domain;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Bungie;

public class GenerateBungieTokenCommand : ICommand
{
    public required string OAuthCode { get; set; }
}

public class GenerateBungieTokenResponse : VoidBotResponse<BungieApiGenerateTokenResponse>
{
}