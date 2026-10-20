using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>本人继续参与世界的角色领取私人或分享掉落；成功回执在玩家装备写操作范围内防重。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class PickupEraEquipmentDropCommand : ICommand, IRequireEraPlayer
{
    /// <summary>由路由覆盖的世界 ID。</summary>
    public Guid WorldInstanceId { get; set; }
    /// <summary>由路由覆盖的当前地面记录 ID。</summary>
    public Guid DropId { get; set; }
    /// <summary>本人在该世界继续参与的角色，服务器校验归属及成员关系。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>与装配、卸下共用玩家范围的成功写操作防重标识。</summary>
    public Guid RequestId { get; set; }
    /// <summary>查询快照中的角色 EquipmentVersion。</summary>
    public long ExpectedEquipmentVersion { get; set; }
    /// <summary>地面记录版本，不是世界或装备实例版本。</summary>
    public long ExpectedDropVersion { get; set; }
    /// <summary>地面装备实例的保存版本。</summary>
    public long ExpectedItemVersion { get; set; }
}

public sealed class PickupEraEquipmentDropResponse : EraEquipmentOperationResponse
{
}
