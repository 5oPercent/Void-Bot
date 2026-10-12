using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraTeamInvitationsRequest : IRequest, IRequireEraPlayer
{
    public bool Outgoing { get; set; } = false;
}

public class GetEraTeamInvitationsResponse : VoidBotResponse<List<EraTeamInvitationDto>>
{
}
