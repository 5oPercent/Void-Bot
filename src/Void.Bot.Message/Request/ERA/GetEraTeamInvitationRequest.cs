using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraTeamInvitationRequest : IRequest, IRequireEraPlayer
{
    public Guid TeamId { get; set; }
    public Guid InvitationId { get; set; }
}

public class GetEraTeamInvitationResponse : VoidBotResponse<EraTeamInvitationDto>
{
}
