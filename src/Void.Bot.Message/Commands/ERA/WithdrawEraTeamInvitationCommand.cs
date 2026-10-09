using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;

namespace Void.Bot.Message.Commands.ERA;

public class WithdrawEraTeamInvitationCommand : ICommand
{
    public Guid TeamId { get; set; }
    public Guid InvitationId { get; set; }
}

public class WithdrawEraTeamInvitationResponse : VoidBotResponse<bool>
{
}
