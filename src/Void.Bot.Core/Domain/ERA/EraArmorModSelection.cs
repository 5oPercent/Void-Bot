using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_armor_mod_selection")]
public sealed class EraArmorModSelection : IEntityBase
{
    public Guid EquipmentId { get; set; }
    public Guid SlotId { get; set; }
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    public int DisplayOrder { get; set; }
    public Guid SelectedCandidateId { get; set; }
}
