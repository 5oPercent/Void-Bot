using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_reward_event")]
[Index(nameof(ScopeId), nameof(SourceType), nameof(SourceInstanceId), nameof(TriggerIndex), nameof(RewardNodeId),
    IsUnique = true, Name = "ux_era_reward_event_identity")]
public sealed class EraRewardEvent : IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public Guid ScopeId { get; set; }
    /// <summary>仅世界奖励填写，内部非世界奖励为空。</summary>
    public Guid? WorldInstanceId { get; set; }
    public EraRewardSourceType SourceType { get; set; }
    public Guid SourceInstanceId { get; set; }
    [StringLength(128)]
    public string SourceDefinitionId { get; set; } = string.Empty;
    public int TriggerIndex { get; set; }
    [StringLength(128)]
    public string RewardNodeId { get; set; } = string.Empty;
    [Column(TypeName = "text")]
    public string RequestPayload { get; set; } = string.Empty;
    public long ConfigurationVersion { get; set; }
    public long ManifestVersion { get; set; }
    public DateTimeOffset AcceptedAt { get; set; }
    [Column(TypeName = "mediumtext")]
    public string PlanPayload { get; set; } = string.Empty;
    /// <summary>仅整批成功才填写；失败保留原计划，不创建成功回执。</summary>
    [Column(TypeName = "mediumtext")]
    public string? ResultPayload { get; set; }
}
