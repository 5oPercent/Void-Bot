using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Discord;

[Table("discord_temporary_voice_channel")]
public class DiscordTemporaryVoiceChannel : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public ulong GuildId { get; set; }

    public Guid ConfigId { get; set; }

    public ulong TriggerChannelId { get; set; }

    public ulong CreateCategoryId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong OwnerUserId { get; set; }

    [StringLength(128)]
    public string OwnerDisplayName { get; set; } = string.Empty;

    public int UserLimit { get; set; }

    public bool IsPrivate { get; set; } = true;

    public DateTimeOffset? LastEmptyAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
