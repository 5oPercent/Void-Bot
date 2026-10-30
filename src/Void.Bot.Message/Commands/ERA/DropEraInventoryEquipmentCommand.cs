using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>本人在有效世界中丢出背包装备，不接受已穿装备；保留原始分享资格和期限。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class DropEraInventoryEquipmentCommand : ICommand, IRequireEraPlayer
{
    /// <summary>由路由覆盖的本人角色 ID。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>由路由覆盖的背包装备 ID。</summary>
    public Guid EquipmentInstanceId { get; set; }
    public Guid WorldInstanceId { get; set; }
    /// <summary>与装配、卸下、拾取共用玩家范围的成功请求标识。</summary>
    public Guid RequestId { get; set; }
    public long ExpectedEquipmentVersion { get; set; }
    public long ExpectedItemVersion { get; set; }
    /// <summary>操作时捕获的世界坐标，后端不校验距离。</summary>
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
}

public sealed class DropEraInventoryEquipmentResponse : EraEquipmentOperationResponse
{
}
