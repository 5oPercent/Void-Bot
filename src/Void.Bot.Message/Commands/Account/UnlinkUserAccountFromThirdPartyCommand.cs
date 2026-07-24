using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Commands.Account;

public class UnlinkUserAccountFromThirdPartyCommand : ICommand
{
    public Guid UserId { get; set; }

    public AppPlatformEnum Platform { get; set; }
}

public class UnlinkUserAccountFromThirdPartyResponse : VoidBotResponse
{
}
