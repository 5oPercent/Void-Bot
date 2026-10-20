using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>内部整批背包发放；只接受已持久化的计划 ID，不接受装备参数，无 HTTP 路由。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class GrantEraInventoryRewardCommand : ICommand
{
    public Guid EventId { get; set; }
}

public sealed class GrantEraInventoryRewardResponse : EraResponse<EraInventoryRewardReceipt>
{
    public bool IsReplay { get; set; }
}
