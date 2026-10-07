using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_player")]
public class EraPlayer : IEntity<Guid>, IEntityCreated, IEntityModified
{
    /// <summary>ERA 玩家档案的独立 ID，同时作为共享仓库的归属 ID。</summary>
    [Key]
    public Guid Id { get; set; }

    public int WarehouseCapacity { get; set; }
    public long Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
