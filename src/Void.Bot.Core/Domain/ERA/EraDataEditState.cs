using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>当前数据的读取一致性和编辑并发状态，不保存业务定义及版本缓存。</summary>
[Table("era_data_edit_state")]
public sealed class EraDataEditState : IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public long Revision { get; set; }
    /// <summary>种族阵营关系的编辑并发令牌。</summary>
    public long SettingsVersion { get; set; }
    /// <summary>从种族阵营关系表临时组合，不映射数据库列。</summary>
    [NotMapped]
    public string SettingsPayload { get; set; } = "";
    public Guid? LastModifiedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
