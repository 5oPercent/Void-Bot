namespace Void.Bot.Message.Dto.Discord;

public class DiscordTemporaryVoiceChannelDto
{
    public Guid Id { get; set; }

    public ulong GuildId { get; set; }

    public Guid ConfigId { get; set; }

    public ulong TriggerChannelId { get; set; }

    public ulong CreateCategoryId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong OwnerUserId { get; set; }

    public string OwnerDisplayName { get; set; } = string.Empty;

    public int UserLimit { get; set; }

    public bool IsPrivate { get; set; }

    public List<ulong> AllowedUserIds { get; set; } = [];

    public DateTimeOffset? LastEmptyAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
