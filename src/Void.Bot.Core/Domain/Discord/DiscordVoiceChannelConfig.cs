using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Discord;

[Table("discord_voice_channel_config")]
public class DiscordVoiceChannelConfig : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public ulong GuildId { get; set; }

    public ulong TriggerChannelId { get; set; }

    public ulong CreateCategoryId { get; set; }

    public int DefaultUserLimit { get; set; } = 3;

    [StringLength(128)]
    public string ChannelNameTemplate { get; set; } = "{user} 的频道";

    public bool Enabled { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
