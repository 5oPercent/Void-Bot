using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_framework_capability")]
public sealed class EraWeaponFrameworkCapability : IEntityBase
{
    public Guid FrameworkId { get; set; }
    public EraWeaponCapabilityKind CapabilityKind { get; set; }
    [Column(TypeName = "text")] public string ConfigurationPayload { get; set; } = string.Empty;
}
