using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;

namespace Void.Bot.Message.Commands.Social;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
public class RemoveFriendCommand : ICommand
{
    public Guid TargetAccountId { get; set; }
}

public class RemoveFriendResponse : VoidBotResponse<bool>
{
}
