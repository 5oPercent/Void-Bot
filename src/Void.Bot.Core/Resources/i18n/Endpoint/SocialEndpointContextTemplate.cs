using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Social")]
public sealed class SocialEndpointContextTemplate : IContextTemplate
{
    public string FindSocialAccount { get; set; } = "查找公开账号";
    public string GetFriends { get; set; } = "获取好友列表";
    public string GetFriendRequests { get; set; } = "获取好友申请";
    public string GetAccountBlocks { get; set; } = "获取拉黑列表";
    public string SendFriendRequest { get; set; } = "发送好友申请";
    public string AcceptFriendRequest { get; set; } = "接受好友申请";
    public string RejectFriendRequest { get; set; } = "拒绝好友申请";
    public string WithdrawFriendRequest { get; set; } = "撤回好友申请";
    public string RemoveFriend { get; set; } = "删除好友";
    public string BlockAccount { get; set; } = "拉黑账号";
    public string UnblockAccount { get; set; } = "解除拉黑";
}
