using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>原子销毁当前世界中本人有拾取权限的一批地面装备，不经过背包。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class DestroyEraEquipmentDropsCommand : ICommand, IRequireEraPlayer
{
    public Guid WorldInstanceId { get; set; }
    public Guid CharacterId { get; set; }
    public Guid RequestId { get; set; }
    public long ExpectedEquipmentVersion { get; set; }
    public List<EraEquipmentDropDestroyItemDto> Items { get; set; } = new();
}

public sealed class DestroyEraEquipmentDropsResponse : EraEquipmentOperationResponse
{
}
