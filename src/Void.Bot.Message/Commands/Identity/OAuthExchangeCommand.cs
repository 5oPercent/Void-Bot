using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Message.Commands.Identity;

public sealed class OAuthExchangeCommand : ICommand
{
    public required string Code { get; init; }
}

public sealed class OAuthExchangeResponse : VoidBotResponse<JwtToken>
{
}
