using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraTeamRequest : IRequest
{
    public Guid TeamId { get; set; }
}

public class GetEraTeamResponse : EraResponse<EraTeamDto>
{
}
