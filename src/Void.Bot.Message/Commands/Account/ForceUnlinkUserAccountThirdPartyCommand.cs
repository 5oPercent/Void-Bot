using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Commands.Account;

public sealed class ForceUnlinkUserAccountThirdPartyCommand : ICommand
{
    public Guid UserId { get; init; }

    public AppPlatformEnum Platform { get; init; }
}

public sealed class ForceUnlinkUserAccountThirdPartyResponse : VoidBotResponse
{
}
