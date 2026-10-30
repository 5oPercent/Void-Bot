using Mediator.Net.Contracts;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>角色装备操作共有的归属、幂等与并发校验字段。</summary>
public interface IEraEquipmentOperation : ICommand, IRequireEraPlayer
{
    /// <summary>本人角色 ID，由路由覆盖。</summary>
    Guid CharacterId { get; set; }
    /// <summary>当前玩家全部装备写操作范围内唯一的请求标识，超时重试复用。</summary>
    Guid RequestId { get; set; }
    /// <summary>装入候选或需要卸下的实际装备实例 ID。</summary>
    Guid EquipmentInstanceId { get; set; }
    /// <summary>目标已穿槽位。</summary>
    EraEquipmentSlot Slot { get; set; }
    /// <summary>读取快照的 EquipmentVersion，不是角色资料版本。</summary>
    long ExpectedEquipmentVersion { get; set; }
    /// <summary>EquipmentInstanceId 对应实例的预期版本。</summary>
    long ExpectedItemVersion { get; set; }
}
