using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>装备编辑并发状态与结构元数据；业务定义保存在武器和护甲各自的关系表，不保存历史快照。</summary>
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
