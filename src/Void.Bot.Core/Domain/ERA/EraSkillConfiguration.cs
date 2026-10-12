using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>单行整批发布，锁固定种子行保证首次发布与后续更新使用同一并发边界。</summary>
[Table("era_skill_configuration")]
public class EraSkillConfiguration : IEntity<Guid>, IEntityModified
{
    [Key]
    public Guid Id { get; set; }
    public long Version { get; set; }

    [Column(TypeName = "mediumtext")]
    public string Payload { get; set; } = "";
    public Guid? LastModifiedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
