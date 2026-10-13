using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>技能草稿编辑与发布持久化记录。</summary>
[Table("era_skill_publish_receipt")]
public sealed class EraSkillPublishReceipt : IEntity<Guid>
{
    /// <summary>稳定发布请求 ID。</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>提交发布的管理员账号。</summary>
    public Guid AccountId { get; set; }

    /// <summary>请求要求的草稿修订号。</summary>
    public long ExpectedDraftRevision { get; set; }

    /// <summary>请求要求的原发布版本。</summary>
    public long ExpectedPublishedVersion { get; set; }

    /// <summary>此请求最终对应的发布版本。</summary>
    public long PublishedVersion { get; set; }

    /// <summary>请求提交成功时间。</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
