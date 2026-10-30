using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_framework")]
public sealed class EraWeaponFramework : IEntity<Guid>
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    [StringLength(2048)] public string Note { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    [StringLength(32)] public string PresentationDriverKey { get; set; } = string.Empty;
}
