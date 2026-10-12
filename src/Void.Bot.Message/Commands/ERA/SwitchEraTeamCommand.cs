using Void.Bot.Message.Request.ERA;
using System.Data;
using Void.Bot.Message.Attributes;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
public class SwitchEraTeamCommand : ICommand, IRequireEraPlayer
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public Guid CharacterId { get; set; }
    public string TeamCode { get; set; } = string.Empty;
}

public class SwitchEraTeamResponse : EraResponse<EraTeamDto>
{
}
