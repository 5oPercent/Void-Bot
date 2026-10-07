using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_configuration")]
[Index(nameof(ConfigKey), IsUnique = true)]
public class EraConfiguration : IEntity<Guid>, IEntityModified
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(128)]
    public string ConfigKey { get; set; } = string.Empty;

    [StringLength(32)]
    public string ValueType { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string Value { get; set; } = string.Empty;

    [StringLength(256)]
    public string Description { get; set; } = string.Empty;

    /// <summary>整套配置的同步版本；同一次发布后的全部配置项使用相同版本。</summary>
    public long ConfigVersion { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? LastModifiedBy { get; set; }
}
