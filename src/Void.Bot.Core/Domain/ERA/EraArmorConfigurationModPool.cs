using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_armor_configuration_mod_pool")]
public sealed class EraArmorConfigurationModPool : IEntityBase
{
    public Guid SlotId { get; set; }
    public Guid ModDefinitionId { get; set; }
    public int Weight { get; set; }
}
