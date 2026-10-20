namespace Void.Bot.Core.Services.ERA.Reward.Models;

public sealed class EraPlannedRewardGrant
{
    public Guid GrantId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public List<EraPlannedRewardEquipment> Equipment { get; set; } = new List<EraPlannedRewardEquipment>();
}
