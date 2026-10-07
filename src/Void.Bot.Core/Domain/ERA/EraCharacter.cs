using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>ERA 玩家档案下的角色实例；多个角色通过 PlayerId 共享同一仓库。</summary>
[Table("era_character")]
[Index(nameof(PlayerId), nameof(CreationRequestId), IsUnique = true)]
public class EraCharacter : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>角色所属的 ERA 玩家档案 ID；禁用角色时保留此关联。</summary>
    public Guid PlayerId { get; set; }

    /// <summary>创建请求的幂等标识，同一玩家重试创建时必须复用。</summary>
    public Guid CreationRequestId { get; set; }

    /// <summary>客户端和官方配置共同识别的角色定义 ID。</summary>
    [StringLength(128)]
    public string CharacterDefinitionId { get; set; } = string.Empty;

    [StringLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>禁用后不可用于游戏，保留角色记录及所属玩家关联。</summary>
    public bool IsDisabled { get; set; }

    public long Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
