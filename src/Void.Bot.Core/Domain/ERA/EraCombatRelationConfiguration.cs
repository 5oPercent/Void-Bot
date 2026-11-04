using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>种族、阵营和默认关系的当前配置，不保存发布版本。</summary>
[Table("era_combat_relation_configuration")]
public sealed class EraCombatRelationConfiguration : IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    [Column(TypeName = "mediumtext")]
    public string Payload { get; set; } = "";
}
