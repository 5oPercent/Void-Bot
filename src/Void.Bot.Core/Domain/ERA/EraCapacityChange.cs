using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>容量操作审计与幂等回执，原始请求字段用于检测 ID 复用冲突。</summary>
[Table("era_capacity_change")]
[Index(nameof(PlayerId), nameof(RequestId), IsUnique = true)]
public class EraCapacityChange : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid RequestId { get; set; }
    public Guid OperatorId { get; set; }
    public bool IsAdjustment { get; set; }
    public int RequestedValue { get; set; }
    public long ExpectedVersion { get; set; }
    [StringLength(512)]
    public string Reason { get; set; } = string.Empty;
    public int PreviousCapacity { get; set; }
    public int WarehouseCapacity { get; set; }
    public long PlayerVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
