using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
public class SendEraTeamInvitationCommand : ICommand
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public Guid TargetAccountId { get; set; }
    public Guid RequestId { get; set; }
}

public class SendEraTeamInvitationResponse : VoidBotResponse<EraTeamInvitationDto>
{
}
