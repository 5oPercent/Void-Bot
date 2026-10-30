using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_equipment_share")]
public sealed class EraEquipmentShare : IEntityBase
{
    [Key]
    public Guid EquipmentInstanceId { get; set; }
    [Column(TypeName = "text")]
    public string AllowedPlayerIdsPayload { get; set; } = string.Empty;
    public DateTimeOffset ShareUntil { get; set; }
}
