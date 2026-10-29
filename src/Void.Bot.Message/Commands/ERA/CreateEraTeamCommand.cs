using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Enum.ERA;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class CreateEraTeamCommand : ICommand, IRequireEraPlayer
{
    public EraTeamJoinMode JoinMode { get; set; } = EraTeamJoinMode.Public;
    public EraTeamInvitationPermission InvitationPermission { get; set; } = EraTeamInvitationPermission.AnyMember;
    public Guid RequestId { get; set; }
    public Guid CharacterId { get; set; }
}

public class CreateEraTeamResponse : EraResponse<EraTeamDto>
{
}
