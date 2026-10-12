using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;

namespace Void.Bot.Message.Commands.ERA;

public class RejectEraTeamInvitationCommand : ICommand, IRequireEraPlayer
{
    public Guid TeamId { get; set; }
    public Guid InvitationId { get; set; }
}

public class RejectEraTeamInvitationResponse : VoidBotResponse<bool>
{
}
