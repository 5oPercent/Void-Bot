using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>可信业务直接指定奖励条目，为世界内指定角色生成正式私人掉落；HTTP 入口使用已有权限机制。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class GrantEraGroundRewardCommand : ICommand
{
    public Guid WorldInstanceId { get; set; }
    /// <summary>同一请求重试保持不变；装备与掉落 ID 均由后端生成。</summary>
    public Guid RequestId { get; set; }
    /// <summary>本次领取角色，重试及房主迁移保持不变；须仍在该世界有效参与。</summary>
    public Guid CharacterId { get; set; }
    public Guid MemberId { get; set; }
    public long AuthorityEpoch { get; set; }
    public List<EraEquipmentRewardEntry> Entries { get; set; } = new List<EraEquipmentRewardEntry>();
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
}
