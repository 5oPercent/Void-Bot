using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Discord;

[Language("Discord", "Voice")]
public sealed class DiscordVoiceContextTemplate : IContextTemplate
{
    public string VoiceChannelConfigNotFound { get; set; } = "语音频道配置不存在。";
    public string UnknownVoiceCommand { get; set; } = "未知命令。";
    public string TargetUserNotFound { get; set; } = "未找到目标用户。";
    public string BotUserNotAllowed { get; set; } = "不能对 bot 用户执行该操作。";
}
