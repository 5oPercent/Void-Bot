using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>玩家范围内唯一的成功写操作回执，与资产变化同事务提交并保留防重依据。</summary>
[Table("era_equipment_operation_receipt")]
[Index(nameof(PlayerId), nameof(RequestId), IsUnique = true, Name = "ux_era_equipment_operation_request")]
public sealed class EraEquipmentOperationReceipt : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public Guid RequestId { get; set; }
    public EraEquipmentOperation Operation { get; set; }
    /// <summary>首版规范化参数快照，用于检测跨操作或跨角色的请求 ID 复用。</summary>
    [Column(TypeName = "text")]
    public string RequestPayload { get; set; } = string.Empty;
    /// <summary>固定成功回执，重试不能用最新状态覆盖该结果。</summary>
    [Column(TypeName = "mediumtext")]
    public string ResultPayload { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
