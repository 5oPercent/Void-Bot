using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
public class JoinEraFriendTeamCommand : ICommand
{
    public Guid FriendAccountId { get; set; }
    public Guid ExpectedTeamId { get; set; }
    public Guid CharacterId { get; set; }
    public Guid? SourceTeamId { get; set; }
    public Guid? SourceMemberId { get; set; }
}

public class JoinEraFriendTeamResponse : VoidBotResponse<EraTeamDto>
{
}
