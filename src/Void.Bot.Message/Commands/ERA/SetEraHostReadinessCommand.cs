using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class SetEraHostReadinessCommand : ICommand
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public long AuthorityEpoch { get; set; }
    public bool CanHost { get; set; }
}

public class SetEraHostReadinessResponse : EraResponse<EraTeamDto>
{
}
