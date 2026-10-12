using Void.Bot.Message.Request.ERA;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class TransferEraTeamMemberCommand : ICommand, IRequireEraPlayer
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public Guid TargetMemberId { get; set; }
    public long AuthorityEpoch { get; set; }
}

public class TransferEraTeamMemberResponse : EraResponse<EraTeamDto>
{
}
