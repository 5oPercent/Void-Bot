using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Account;

public sealed class SendUserAccountPasswordResetLinkCommand : ICommand
{
    public Guid UserId { get; init; }
}

public sealed class SendUserAccountPasswordResetLinkResponse : VoidBotResponse
{
}
