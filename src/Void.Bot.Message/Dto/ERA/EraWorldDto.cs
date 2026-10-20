using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>当前世界状态；恢复返回当前状态，不重新分配世界身份。</summary>
public sealed class EraWorldDto
{
    /// <summary>后端分配的独立身份，不等于队伍、地图定义或种子。</summary>
    public Guid WorldInstanceId { get; set; }
    /// <summary>当前世界绑定的既有 Redis 队伍。</summary>
    public Guid TeamId { get; set; }
    /// <summary>登记时固定的定义标识，不能作为奖励发生的可信证明。</summary>
    public string WorldDefinitionId { get; set; } = string.Empty;
    /// <summary>持久生命周期状态；构建完成才进入 Ready。</summary>
    public EraWorldState State { get; set; }
    /// <summary>最近一次成功登记／恢复的房主；实时队伍权限仍须在写入时核验。</summary>
    public Guid HostPlayerId { get; set; }
    /// <summary>最近成功恢复的既有队伍代次；不自动修复为当前 Redis 值。</summary>
    public long AuthorityEpoch { get; set; }
    /// <summary>成员、生命周期、权限恢复或限制变化时递增的世界版本。</summary>
    public long Version { get; set; }
    /// <summary>禁止操作已穿装备，空槽装配仍允许。</summary>
    public bool IsEquippedOperationsRestricted { get; set; }
    /// <summary>限制值改变时递增，迁移保持原值；与装备版本分别比较。</summary>
    public long RestrictionVersion { get; set; }
    /// <summary>服务器首次登记时间，重试不刷新。</summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>世界结束或构建失败时间，未结束为空。</summary>
    public DateTimeOffset? EndedAt { get; set; }
    /// <summary>首次结束原因，重复结束不覆盖；历史旧数据可为空。</summary>
    public EraWorldEndReason? EndReason { get; set; }
    /// <summary>后端最近确认活动的服务器时间，不参与业务版本比较。</summary>
    public DateTimeOffset LastActiveAt { get; set; }
    /// <summary>继续成员建议发送世界心跳的间隔，单位秒。</summary>
    public int HeartbeatIntervalSeconds { get; set; }
    /// <summary>无活动宽限期，超过后仍需后台确认，不保证到时立即结束。</summary>
    public int InactivityTimeoutSeconds { get; set; }
    /// <summary>包含已离开历史成员的固定身份及继续状态。</summary>
    public List<EraWorldMemberDto> Members { get; set; } = new List<EraWorldMemberDto>();
}
