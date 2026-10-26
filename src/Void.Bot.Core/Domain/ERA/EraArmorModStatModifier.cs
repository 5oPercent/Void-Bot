using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_armor_mod_stat_modifier")]
public sealed class EraArmorModStatModifier : IEntityBase
{
    public Guid ModDefinitionId { get; set; }
    public int Sequence { get; set; }
    /// <summary>护甲六维属性标识，取值见 EraArmorAttribute。</summary>
    public EraArmorAttribute Stat { get; set; }
    /// <summary>固定加值或加算百分比；百分比用小数表示，0.2 表示增加 20%。</summary>
    public EraArmorModifierOperation Operation { get; set; }
    public double Value { get; set; }
}
