using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Services.ERA.Models;

/// <summary>首次接受即保存的计划；不是正式装备或成功回执，重试不重新生成属性。</summary>
public sealed class EraInventoryRewardPlan
{
    public int SchemaVersion { get; set; } = 1;
    public Guid EventId { get; set; }
    public long ConfigurationVersion { get; set; }
    public long ManifestVersion { get; set; }
    public DateTimeOffset AcceptedAt { get; set; }
    /// <summary>首次使用的来源节点及池完整快照，供审计；发放只消费已抽出的 Equipment。</summary>
    public EraRewardNode Node { get; set; } = new EraRewardNode();
    public EraRewardPool Pool { get; set; } = new EraRewardPool();
    public List<EraPlannedRewardGrant> Grants { get; set; } = new List<EraPlannedRewardGrant>();
}
