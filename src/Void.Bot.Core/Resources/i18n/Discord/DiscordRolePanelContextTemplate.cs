using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Discord;

[Language("Discord", "DiscordRolePanel")]
public sealed class DiscordRolePanelContextTemplate : IContextTemplate
{
    public string EveryoneRoleUnassignableReason { get; set; } = "@everyone 角色无法分配";
    public string ManagedRoleUnassignableReason { get; set; } = "托管角色无法分配";
    public string HigherRoleUnassignableReason { get; set; } = "角色等级高于机器人最高角色";
    public string ChannelIdInvalid { get; set; } = "频道 ID 无效。";
    public string TextChannelUnavailable { get; set; } = "Discord 文本频道不可用。";
    public string ItemsInvalid { get; set; } = "角色面板按钮配置无效。";
    public string InvalidButtonSettings { get; set; } = "角色面板包含重复或不支持的按钮配置。";
    public string UnassignableRoles { get; set; } = "存在无法由机器人分配的角色。";
    public string PanelExpired { get; set; } = "该角色面板已失效。";
    public string AlreadyHasRole { get; set; } = "你已经拥有该角色。";
    public string RoleRemoved { get; set; } = "已移除角色：{0}";
    public string RoleAdded { get; set; } = "已添加角色：{0}";
    public string PanelOrChannelNotFound { get; set; } = "角色面板或频道不存在。";
    public string PanelAlreadyPublished { get; set; } = "角色面板已经发布。";
    public string PublishedPanelNotFound { get; set; } = "已发布的角色面板不存在。";
}
