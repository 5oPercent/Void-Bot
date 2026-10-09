using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraSocial")]
public sealed class EraSocialEndpointContextTemplate : IContextTemplate
{
    public string GetEraFriendStatuses { get; set; } = "批量查询好友 ERA 状态";
    public string GetEraFriendTarget { get; set; } = "查询好友队伍摘要";
    public string JoinEraFriendTeam { get; set; } = "加入好友队伍";
    public string UpdateEraTeamSettings { get; set; } = "修改队伍加入和邀请权限";
    public string SendEraTeamInvitation { get; set; } = "发送队伍邀请";
    public string GetEraTeamInvitations { get; set; } = "获取有效队伍邀请";
    public string GetEraTeamInvitation { get; set; } = "查询队伍邀请";
    public string WithdrawEraTeamInvitation { get; set; } = "撤回队伍邀请";
    public string RejectEraTeamInvitation { get; set; } = "拒绝队伍邀请";
    public string AcceptEraTeamInvitation { get; set; } = "接受队伍邀请";
}
