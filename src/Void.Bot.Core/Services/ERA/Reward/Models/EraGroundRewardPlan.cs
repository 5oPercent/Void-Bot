using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Services.ERA.Reward.Models;

/// <summary>与地面资产在同一事务固定的配置、名单及原始位置；重试只返回成功回执。</summary>
public sealed class EraGroundRewardPlan
{
    public int SchemaVersion { get; set; } = 1;
    public EraRewardNode? Node { get; set; }
    public EraRewardPool? Pool { get; set; }
    public List<EraEquipmentRewardEntry>? Entries { get; set; }
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
    public List<Guid> AllowedPlayerIds { get; set; } = new List<Guid>();
}
