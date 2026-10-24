using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>将指定已穿装备卸下到角色共用背包，背包已满时拒绝。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class UnequipEraEquipmentCommand : IEraEquipmentOperation
{
    /// <summary>由路由覆盖的本人角色 ID。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>玩家范围内的幂等标识，同一成功请求重试不能改参数。</summary>
    public Guid RequestId { get; set; }
    /// <summary>目标槽位当前已穿实例 ID。</summary>
    public Guid EquipmentId { get; set; }
    /// <summary>需要卸下的已穿槽位。</summary>
    public EraEquipmentSlot Slot { get; set; }
    /// <summary>角色装备快照的预期版本。</summary>
    public long ExpectedEquipmentVersion { get; set; }
    /// <summary>已穿实例的预期版本。</summary>
    public long ExpectedItemVersion { get; set; }
}

/// <summary>返回本次成功操作的固定回执；IsReplay 为 true 时 State 是历史结果。</summary>
public sealed class UnequipEraEquipmentResponse : EraEquipmentOperationResponse
{
}
