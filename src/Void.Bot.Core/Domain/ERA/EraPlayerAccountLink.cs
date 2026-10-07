using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>Void.Bot 主账号与 ERA 玩家档案的一对一绑定关系。</summary>
[Table("era_player_account_link")]
[Index(nameof(AccountId), IsUnique = true)]
[Index(nameof(PlayerId), IsUnique = true)]
public class EraPlayerAccountLink : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>Void.Bot 主账号 ID。</summary>
    public Guid AccountId { get; set; }

    /// <summary>独立的 ERA 玩家档案 ID。</summary>
    public Guid PlayerId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
