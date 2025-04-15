using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie;

namespace Void.Bot.Message.Commands.Bungie;

public class RefreshBungieTokenCommand : ICommand
{
    public required string Token { get; set; }
}

public class RefreshBungieTokenResponse : VoidBotResponse<BungieApiGenerateTokenResponse>
{
}