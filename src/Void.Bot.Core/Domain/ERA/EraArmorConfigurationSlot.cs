using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_armor_configuration_slot")]
public sealed class EraArmorConfigurationSlot : IEntity<Guid>
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConfigurationId { get; set; }
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    public int CandidateCount { get; set; }
    public int DisplayOrder { get; set; }
}
