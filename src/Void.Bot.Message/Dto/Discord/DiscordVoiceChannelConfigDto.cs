namespace Void.Bot.Message.Dto.Discord;

public class DiscordVoiceChannelConfigDto
{
    public Guid Id { get; set; }

    public string GuildId { get; set; } = string.Empty;

    public string TriggerChannelId { get; set; } = string.Empty;

    public string CreateCategoryId { get; set; } = string.Empty;

    public int DefaultUserLimit { get; set; }

    public string ChannelNameTemplate { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
