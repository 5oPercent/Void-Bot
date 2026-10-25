using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_configuration_mod_pool")]
public sealed class EraWeaponConfigurationModPool : IEntityBase
{
    public Guid SlotId { get; set; }
    public Guid ModDefinitionId { get; set; }
    public int Weight { get; set; }
}
