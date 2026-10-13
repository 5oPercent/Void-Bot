namespace Void.Bot.Message.Dto.Social;

/// <summary>
/// SignalR 社交数据失效通知。客户端据此通过 HTTP 查询最新数据，通知本身不提供业务授权。
/// </summary>
public class SocialNotificationDto
{
    /// <summary>通知唯一标识，供客户端去重；同一通知重试投递时保持不变。</summary>
    public Guid EventId { get; set; }
    /// <summary>变化类型，例如好友申请变化或 ERA 队伍对端变化，决定需要刷新的数据。</summary>
    public string EventType { get; set; } = string.Empty;
    /// <summary>变化资源标识；具体含义由事件类型决定，例如申请 ID、好友账号 ID 或队伍 ID。</summary>
    public Guid ResourceId { get; set; }
    /// <summary>所属资源标识；队伍邀请通知为队伍 ID，无所属资源时为空。</summary>
    public Guid? ParentResourceId { get; set; }
    /// <summary>
    /// 资源变化版本，仅在相同事件类型和资源内比较；对端通知还需结合主机代次，不是全局版本。
    /// </summary>
    public long ResourceVersion { get; set; }
    /// <summary>
    /// ERA 队伍对端变化所属的主机代次，用于隔离旧代次通知；其他事件类型不使用此字段时为空。
    /// </summary>
    public long? AuthorityEpoch { get; set; }
    /// <summary>服务端生成该变化通知的 UTC 时间，供延迟统计使用，不作为资源版本。</summary>
    public DateTimeOffset OccurredAt { get; set; }
    /// <summary>关联业务请求标识；没有对应业务请求的通知使用空 GUID。</summary>
    public Guid CorrelationId { get; set; }
}
