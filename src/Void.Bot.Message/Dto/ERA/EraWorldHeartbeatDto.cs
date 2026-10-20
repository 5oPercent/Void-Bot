namespace Void.Bot.Message.Dto.ERA;

/// <summary>世界成员的存活确认，不改变世界业务版本。</summary>
public sealed class EraWorldHeartbeatDto
{
    public Guid WorldInstanceId { get; set; }
    /// <summary>服务器最近确认活动的时间，不能由客户端指定。</summary>
    public DateTimeOffset LastActiveAt { get; set; }
    /// <summary>建议心跳间隔。</summary>
    public int HeartbeatIntervalSeconds { get; set; }
    /// <summary>无活动宽限期；超过后仍须后端核验，不等同于保证结束时间。</summary>
    public int InactivityTimeoutSeconds { get; set; }
}
