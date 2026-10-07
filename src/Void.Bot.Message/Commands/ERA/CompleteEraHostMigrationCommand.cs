using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class CompleteEraHostMigrationCommand : ICommand
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public long AuthorityEpoch { get; set; }
    public bool Success { get; set; }
}

public class CompleteEraHostMigrationResponse : EraResponse<EraTeamDto>
{
}
