using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>技能草稿编辑与发布持久化记录。</summary>
[Table("era_skill_draft_record")]
[Index(nameof(WorkspaceId),nameof(Kind),nameof(StableId),IsUnique=true)]
public sealed class EraSkillDraftRecord : IEntity<Guid>
{
    /// <summary>持久化行标识。</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>所属共享草稿工作区。</summary>
    public Guid WorkspaceId { get; set; }

    /// <summary>体系／技能／BUFF 类别。</summary>
    public EraSkillDraftKind Kind { get; set; }

    /// <summary>创建后固定的业务 ID。</summary>
    [MaxLength(128)]
    public string StableId { get; set; } = "";

    /// <summary>当前对象版本。</summary>
    public long Version { get; set; }

    /// <summary>该对象独立 JSON 定义。</summary>
    [Column(TypeName = "mediumtext")]
    public string Payload { get; set; } = "";

    /// <summary>是否作草稿删除标记。</summary>
    public bool IsDeleted { get; set; }

    /// <summary>此稳定 ID 是否进入过已发布配置。</summary>
    public bool WasPublished { get; set; }

    /// <summary>最后编辑账号。</summary>
    public Guid? LastModifiedBy { get; set; }

    /// <summary>最后编辑时间。</summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
