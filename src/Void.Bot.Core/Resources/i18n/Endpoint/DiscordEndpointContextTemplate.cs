using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Discord")]
public sealed class DiscordEndpointContextTemplate : IContextTemplate
{
    public string StartBotAsync { get; set; } = "启动 Discord 机器人";
    public string StopBotAsync { get; set; } = "停止 Discord 机器人";
    public string ApplyGlobalSlashCommandAsync { get; set; } = "应用全局 Slash 命令";
    public string ApplyGuildSlashCommandAsync { get; set; } = "应用公会 Slash 命令";
    public string GetVoiceChannelConfigAsync { get; set; } = "获取语音频道配置";
    public string GetVoiceChannelConfigListAsync { get; set; } = "获取语音频道配置列表";
    public string AddVoiceChannelConfigAsync { get; set; } = "新增语音频道配置";
    public string UpdateVoiceChannelConfigAsync { get; set; } = "更新语音频道配置";
    public string DeleteVoiceChannelConfigAsync { get; set; } = "删除语音频道配置";
    public string GetTemporaryVoiceChannelListAsync { get; set; } = "获取临时语音频道列表";

}
