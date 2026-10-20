using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>每个节点事件每玩家一份，多个装备关联该份成功发放；不级联删除历史。</summary>
[Table("era_reward_grant")]
[Index(nameof(EventId), nameof(PlayerId), IsUnique = true, Name = "ux_era_reward_grant_player")]
public sealed class EraRewardGrant : IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public DateTimeOffset GrantedAt { get; set; }
    [Column(TypeName = "mediumtext")]
    public string ResultPayload { get; set; } = string.Empty;
}
