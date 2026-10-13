using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>技能草稿编辑与发布持久化记录。</summary>
[Table("era_skill_draft_workspace")]
public sealed class EraSkillDraftWorkspace : IEntity<Guid>
{
    /// <summary>固定共享工作区标识。</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>是否已经从当前发布配置初始化。</summary>
    public bool Initialized { get; set; }

    /// <summary>所有编辑操作共同递增的草稿修订号。</summary>
    public long Revision { get; set; }

    /// <summary>草稿最近一次发布或初始化对应的版本。</summary>
    public long BasePublishedVersion { get; set; }

    /// <summary>最近一次发布包含的草稿修订号。</summary>
    public long PublishedRevision { get; set; }

    /// <summary>工作区设置的单项版本。</summary>
    public long SettingsVersion { get; set; }

    /// <summary>通用战术池和禁用列表的 JSON。</summary>
    [Column(TypeName = "mediumtext")]
    public string SettingsPayload { get; set; } = "";

    /// <summary>最后编辑账号。</summary>
    public Guid? LastModifiedBy { get; set; }

    /// <summary>最后编辑时间。</summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
