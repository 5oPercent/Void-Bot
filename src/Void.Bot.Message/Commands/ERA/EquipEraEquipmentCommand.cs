using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>从背包装配，或原子移动／互换已穿武器；成功回执与全部位置变化一起提交。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class EquipEraEquipmentCommand : IEraEquipmentOperation
{
    /// <summary>由路由覆盖的本人角色 ID。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>玩家范围内的幂等标识，同一成功请求重试不能改参数。</summary>
    public Guid RequestId { get; set; }
    /// <summary>当前角色背包或已穿武器中的候选实例 ID。</summary>
    public Guid EquipmentId { get; set; }
    /// <summary>目标已穿槽位。</summary>
    public EraEquipmentSlot Slot { get; set; }
    /// <summary>候选当前已穿位置；背包候选必须为 null。</summary>
    public EraEquipmentSlot? ExpectedSourceSlot { get; set; }
    /// <summary>角色装备快照的预期版本。</summary>
    public long ExpectedEquipmentVersion { get; set; }
    /// <summary>候选实例的预期版本。</summary>
    public long ExpectedItemVersion { get; set; }
    /// <summary>目标槽位原装备；空槽必须为 null。</summary>
    public Guid? ExpectedEquippedEquipmentId { get; set; }
    /// <summary>原装备预期版本；空槽必须为 null。</summary>
    public long? ExpectedEquippedVersion { get; set; }
    /// <summary>选取候选时使用的装备目录发布版本。</summary>
    public long ExpectedManifestVersion { get; set; }
}

/// <summary>返回本次成功操作的固定回执；IsReplay 为 true 时 State 是历史结果。</summary>
public sealed class EquipEraEquipmentResponse : EraEquipmentOperationResponse
{
}
