using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_template_stat")]
public sealed class EraWeaponTemplateStat : IEntityBase
{
    public Guid TemplateId { get; set; }
    public EraWeaponStat Stat { get; set; }
    public double Value { get; set; }
    public double? EffectAtZero { get; set; }
    public double? EffectAtHundred { get; set; }
}
