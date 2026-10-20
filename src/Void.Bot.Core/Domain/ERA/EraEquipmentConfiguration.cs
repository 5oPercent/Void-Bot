using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>整批版本化装备目录，沿用技能目录的固定配置行与 JSON 载荷方式。</summary>
[Table("era_equipment_configuration")]
public sealed class EraEquipmentConfiguration : IEntity<Guid>, IEntityModified
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
