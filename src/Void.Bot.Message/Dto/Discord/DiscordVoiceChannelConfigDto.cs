namespace Void.Bot.Message.Dto.Discord;

public class DiscordVoiceChannelConfigDto
{
    public Guid Id { get; set; }

    public ulong GuildId { get; set; }

    public ulong TriggerChannelId { get; set; }

    public ulong CreateCategoryId { get; set; }

    public int DefaultUserLimit { get; set; }

    public string ChannelNameTemplate { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
