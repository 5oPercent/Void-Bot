using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_armor_template")]
public sealed class EraArmorTemplate : IEntity<Guid>
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    [StringLength(256)] public string Icon { get; set; } = string.Empty;
    [StringLength(2048)] public string Note { get; set; } = string.Empty;
    public bool IsGenerationEnabled { get; set; }
    public Guid? SlotConfigurationId { get; set; }
    [Column(TypeName = "text")] public string CompatibleSlotsPayload { get; set; } = string.Empty;
}
