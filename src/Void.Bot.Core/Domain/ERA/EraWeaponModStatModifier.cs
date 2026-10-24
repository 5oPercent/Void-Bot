using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_mod_stat_modifier")]
public sealed class EraWeaponModStatModifier : IEntityBase
{
    public Guid ModDefinitionId { get; set; }
    public int Sequence { get; set; }
    public EraWeaponStat Stat { get; set; }
    public EraWeaponModifierOperation Operation { get; set; }
    public double Value { get; set; }
}
