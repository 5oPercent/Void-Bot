using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Account;

public sealed class BeginTwoFactorSetupCommand : ICommand
{
    public required string Password { get; init; }
}

public sealed class BeginTwoFactorSetupResponse : VoidBotResponse<TwoFactorSetupDto>
{
}

public sealed class TwoFactorSetupDto
{
    public required string ManualEntryKey { get; init; }

    public required string OtpAuthUri { get; init; }
}
