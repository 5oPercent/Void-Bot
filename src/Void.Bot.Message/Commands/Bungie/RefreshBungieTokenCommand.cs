using Mediator.Net.Contracts;
using Void.Bot.Message.Commands.Bungie.Domain;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Bungie;

public class RefreshBungieTokenCommand : ICommand
{
    public required string Token { get; set; }
}

public class RefreshBungieTokenResponse : VoidBotResponse<BungieGenerateTokenResponseDto>
{
}