using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraWorld")]
public sealed class EraWorldContextTemplate : IContextTemplate
{
    public string InvalidOperation { get; set; } = "世界操作参数无效";
    public string NotFound { get; set; } = "世界不存在或当前玩家不是该世界成员";
    public string TeamUnavailable { get; set; } = "当前队伍不可用，请刷新队伍状态";
    public string AuthorityChanged { get; set; } = "房主或代次已经变化，请刷新并恢复原世界";
    public string HostRequired { get; set; } = "只有当前房主可以执行该操作";
    public string VersionConflict { get; set; } = "世界状态已变化，请刷新后重试";
    public string AlreadyActive { get; set; } = "队伍或成员已有正在继续的世界";
    public string CreationConflict { get; set; } = "世界登记请求标识已用于其他参数";
    public string Ended { get; set; } = "世界已结束，不能继续操作";
    public string MemberUnavailable { get; set; } = "角色或世界成员状态无效，不能加入或恢复";
    public string NetworkingUnavailable { get; set; } = "世界权限服务暂不可用";
}
