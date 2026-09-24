using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Message.Commands.Identity;

public sealed class VerifyTwoFactorCommand : ICommand
{
    public required string Code { get; init; }
}

public sealed class VerifyTwoFactorResponse : VoidBotResponse<JwtToken>
{
}
