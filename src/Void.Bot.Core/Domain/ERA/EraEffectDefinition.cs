using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>效果当前定义；保存直接生效，不保存历史版本。</summary>
[Table("era_effect_definition")]
[Index(nameof(StableId), IsUnique = true)]
public sealed class EraEffectDefinition : IEraDataDefinition
{
    [Key]
    public Guid Id { get; set; }
    [NotMapped]
    public EraDataKind Kind => EraDataKind.Buff;
    [MaxLength(128)]
    public string StableId { get; set; } = "";
    /// <summary>编辑并发令牌，不代表可查询的历史配置版本。</summary>
    public long Version { get; set; }
    /// <summary>效果禁用状态，独立存储于效果表。</summary>
    public bool IsDisabled { get; set; }
    [Column(TypeName = "mediumtext")]
    public string Payload { get; set; } = "";
    public Guid? LastModifiedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
