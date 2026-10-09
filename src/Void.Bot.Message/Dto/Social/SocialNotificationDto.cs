namespace Void.Bot.Message.Dto.Social;

/// <summary>只携带变化标识，客户端通过 HTTP 读取授权后的最新数据。</summary>
public class SocialNotificationDto
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public Guid? ParentResourceId { get; set; }
    public long ResourceVersion { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid CorrelationId { get; set; }
}
