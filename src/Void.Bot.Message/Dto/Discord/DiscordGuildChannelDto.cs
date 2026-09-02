namespace Void.Bot.Message.Dto.Discord;

public sealed class DiscordGuildChannelDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? ParentId { get; set; }

    public int Position { get; set; }

    public bool UsableAsTrigger { get; set; }

    public bool UsableAsCreateCategory { get; set; }

    public bool UsableAsRolePanel { get; set; }

    public bool Configured { get; set; }

    public Guid? ConfigId { get; set; }
}
