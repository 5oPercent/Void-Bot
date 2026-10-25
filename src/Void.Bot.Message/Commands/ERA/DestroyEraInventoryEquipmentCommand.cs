using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>原子销毁本人角色背包中的一批装备，保留实例与历史记录。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class DestroyEraInventoryEquipmentCommand : ICommand, IRequireEraPlayer
{
    public Guid CharacterId { get; set; }
    public Guid RequestId { get; set; }
    public long ExpectedEquipmentVersion { get; set; }
    public List<EraEquipmentDestructionItemDto> Items { get; set; } = new();
}

public sealed class DestroyEraInventoryEquipmentResponse : EraEquipmentOperationResponse
{
}
