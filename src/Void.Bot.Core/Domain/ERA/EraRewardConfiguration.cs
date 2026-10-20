using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_reward_configuration")]
public sealed class EraRewardConfiguration : IEntity<Guid>, IEntityModified
{
    [Key]
    public Guid Id { get; set; }
    [ConcurrencyCheck]
    public long Version { get; set; }
    [Column(TypeName = "mediumtext")]
    public string Payload { get; set; } = string.Empty;
    public Guid? LastModifiedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
