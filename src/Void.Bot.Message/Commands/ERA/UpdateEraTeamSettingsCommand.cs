using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class UpdateEraTeamSettingsCommand : ICommand
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public long ExpectedVersion { get; set; }
    public EraTeamJoinMode JoinMode { get; set; }
    public EraTeamInvitationPermission InvitationPermission { get; set; }
}

public class UpdateEraTeamSettingsResponse : VoidBotResponse<EraTeamDto>
{
}
