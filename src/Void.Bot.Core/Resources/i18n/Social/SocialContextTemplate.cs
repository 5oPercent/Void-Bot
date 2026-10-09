using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Social;

[Language("Social", "Social")]
public sealed class SocialContextTemplate : IContextTemplate
{
    public string Unauthorized { get; set; } = "请先登录有效账号";
    public string AccountNotFound { get; set; } = "账号不存在或当前不可访问";
    public string SelfOperation { get; set; } = "不能对自己执行此操作";
    public string Blocked { get; set; } = "当前账号关系不允许此操作";
    public string AlreadyFriends { get; set; } = "双方已经是好友";
    public string IncomingRequest { get; set; } = "对方已经发送好友申请，请先处理该申请";
    public string RequestNotFound { get; set; } = "申请不存在或当前账号无权处理";
    public string RequestProcessed { get; set; } = "申请已被处理";
    public string IdempotencyConflict { get; set; } = "请求标识已经用于其他操作";
    public string LimitReached { get; set; } = "好友或待处理申请数量已达到上限";
    public string RateLimited { get; set; } = "请求过于频繁，请稍后重试";
    public string TeamUnavailable { get; set; } = "目标队伍已变化、不可加入或邀请已经失效";
    public string TeamHostRequired { get; set; } = "只有当前主机可以执行此操作";
    public string SoloRequiresOneMember { get; set; } = "只有队内仅剩自己时才能切换为单人";
    public string NetworkingUnavailable { get; set; } = "协调存储暂不可用，请稍后重试";
}
