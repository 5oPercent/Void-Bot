using System.Text.Json;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public sealed record ApproveEraModularActivity(Guid MemberId, long AuthorityEpoch, JsonElement Selection) : ICommand
{
    public Guid WorldInstanceId { get; set; }
}

public sealed record ClaimEraModularActivity(Guid MemberId, long AuthorityEpoch, Guid InitiatorMemberId, Guid ApprovalId) : ICommand
{
    public Guid WorldInstanceId { get; set; }
}

public sealed class ApproveEraModularActivityResponse : EraResponse<Guid>
{
}

public sealed class ClaimEraModularActivityResponse : EraResponse<JsonElement>
{
}
