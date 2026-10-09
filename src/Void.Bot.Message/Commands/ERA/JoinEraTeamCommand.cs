using System.Data;
using Void.Bot.Message.Attributes;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
public class JoinEraTeamCommand : ICommand
{
    public string TeamCode { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
}

public class JoinEraTeamResponse : EraResponse<EraTeamDto>
{
}
