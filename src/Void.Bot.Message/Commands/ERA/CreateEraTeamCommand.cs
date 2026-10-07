using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class CreateEraTeamCommand : ICommand
{
    public Guid RequestId { get; set; }
    public Guid CharacterId { get; set; }
    public int MaxMembers { get; set; } = 4;
}

public class CreateEraTeamResponse : EraResponse<EraTeamDto>
{
}
