using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Discord")]
public sealed class DiscordEndpointContextTemplate : IContextTemplate
{
    public string StartBot { get; set; } = "启动 Discord 机器人";
    public string StopBot { get; set; } = "停止 Discord 机器人";
    public string ApplyGlobalSlashCommand { get; set; } = "应用全局 Slash 命令";
    public string ApplyGuildSlashCommand { get; set; } = "应用公会 Slash 命令";
    public string GetVoiceChannelConfig { get; set; } = "获取语音频道配置";
    public string GetVoiceChannelConfigList { get; set; } = "获取语音频道配置列表";
    public string AddVoiceChannelConfig { get; set; } = "新增语音频道配置";
    public string UpdateVoiceChannelConfig { get; set; } = "更新语音频道配置";
    public string DeleteVoiceChannelConfig { get; set; } = "删除语音频道配置";
    public string GetTemporaryVoiceChannelList { get; set; } = "获取临时语音频道列表";
    public string GetManagedGuildList { get; set; } = "获取可管理的 Discord 公会列表";
    public string GetGuildChannelList { get; set; } = "获取 Discord 公会频道列表";

}
