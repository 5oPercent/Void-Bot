using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Message.Commands.Social;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
public class SendFriendRequestCommand : ICommand
{
    public Guid TargetAccountId { get; set; }
    public Guid RequestId { get; set; }
}

public class SendFriendRequestResponse : VoidBotResponse<FriendRequestDto>
{
}
