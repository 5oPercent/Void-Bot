using Void.Bot.Message.Enum.ERA;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class CreateEraTeamCommand : ICommand
{
    public EraTeamJoinMode JoinMode { get; set; } = EraTeamJoinMode.Public;
    public EraTeamInvitationPermission InvitationPermission { get; set; } = EraTeamInvitationPermission.HostOnly;
    public Guid RequestId { get; set; }
    public Guid CharacterId { get; set; }
}

public class CreateEraTeamResponse : EraResponse<EraTeamDto>
{
}
