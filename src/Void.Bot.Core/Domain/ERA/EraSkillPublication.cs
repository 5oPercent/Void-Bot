using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>技能草稿编辑与发布持久化记录。</summary>
[Table("era_skill_publication")]
[Index(nameof(Version),IsUnique=true)]
public sealed class EraSkillPublication : IEntity<Guid>
{
    /// <summary>历史行标识。</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>唯一已发布版本。</summary>
    public long Version { get; set; }

    /// <summary>本次发布冻结的草稿修订号。</summary>
    public long DraftRevision { get; set; }

    /// <summary>不可变完整配置 JSON。</summary>
    [Column(TypeName = "mediumtext")]
    public string Payload { get; set; } = "";

    /// <summary>不包含版本号的内容摘要。</summary>
    [MaxLength(64)]
    public string ContentHash { get; set; } = "";

    /// <summary>发布账号。</summary>
    public Guid? PublishedBy { get; set; }

    /// <summary>发布时间。</summary>
    public DateTimeOffset PublishedAt { get; set; }
}
