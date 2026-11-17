using System.Text.Json;
using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>申请世界操作授权；管理员操作的 Selection.kind 为非空字符串，具体类型和参数由游戏主机解释，无需在后端注册。equipment 保留给后端装备发放流程。</summary>
public sealed record ApproveEraModularActivity(Guid MemberId, long AuthorityEpoch, JsonElement Selection) : ICommand
{
    public Guid WorldInstanceId { get; set; }
    [JsonIgnore] public bool IsAdminWorldAction { get; set; }
}

public sealed record ClaimEraModularActivity(Guid MemberId, long AuthorityEpoch, Guid InitiatorMemberId, Guid ApprovalId) : ICommand
{
    public Guid WorldInstanceId { get; set; }
    [JsonIgnore] public bool IsAdminWorldAction { get; set; }
}

public sealed class ApproveEraModularActivityResponse : EraResponse<Guid>
{
}

public sealed class ClaimEraModularActivityResponse : EraResponse<JsonElement>
{
}
