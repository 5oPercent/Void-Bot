using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;

namespace Void.Bot.Message.Commands.Social;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
public class AcceptFriendRequestCommand : ICommand
{
    public Guid FriendRequestId { get; set; }
}

public class AcceptFriendRequestResponse : VoidBotResponse<bool>
{
}
