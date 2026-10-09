using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "Era")]
public sealed class EraContextTemplate : IContextTemplate
{
    public string Unauthorized { get; set; } = "请先登录有效账号";
    public string PlayerNotFound { get; set; } = "ERA 玩家尚未初始化或不存在";
    public string CharacterLimitReached { get; set; } = "可用角色数量已达到上限";
    public string CharacterNotFound { get; set; } = "ERA 角色不存在或不属于当前玩家";
    public string ConfigUnavailable { get; set; } = "ERA 配置尚未初始化";
    public string VersionConflict { get; set; } = "数据版本已变化，请重新读取后操作";
    public string IdempotencyConflict { get; set; } = "请求标识已用于不同的操作";
    public string CapacityOutOfRange { get; set; } = "仓库容量不能为负或超出支持范围";
    public string NetworkingRateLimited { get; set; } = "请求过于频繁，请稍后重试";
    public string NetworkingUnavailable { get; set; } = "ERA 协调存储暂不可用，请稍后重试";
    public string NetworkingDisabled { get; set; } = "ERA 队伍与打洞服务尚未启用";
    public string TeamJoinDenied { get; set; } = "当前队伍权限不允许加入或邀请已失效";
    public string TeamNotFound { get; set; } = "队伍已过期、不存在或当前玩家不是成员";
    public string TeamMembershipChanged { get; set; } = "队伍成员会话已变化，请使用重新加入后返回的成员 ID";
    public string TeamAlreadyJoined { get; set; } = "玩家已加入队伍，请先退出";
    public string TeamCapacity { get; set; } = "队伍人数超出限制或已经满员";
    public string TeamMigrating { get; set; } = "队伍正在迁移主机，请稍后加入";
    public string AuthorityChanged { get; set; } = "队伍权威版本已变化，请重新读取队伍";
    public string TeamHostRequired { get; set; } = "只有当前队长可以管理队伍成员";
    public string TeamMemberNotFound { get; set; } = "目标队员不存在、已离队或不能操作自己";
    public string TeamHostNotReady { get; set; } = "目标队员尚未完成世界同步，暂时不能接任队长";
    public string MigrationHostRequired { get; set; } = "只有当前候补主机可以确认接管";
}
