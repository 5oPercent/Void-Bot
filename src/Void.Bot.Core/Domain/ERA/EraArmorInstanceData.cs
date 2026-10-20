using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>一件护甲唯一的最终生成结果，不存放主次排序、临时 BUFF 或运行时生效值。</summary>
[Table("era_armor_instance_data")]
public sealed class EraArmorInstanceData : IEntityBase
{
    [Key]
    public Guid EquipmentId { get; set; }
    public int GenerationRuleVersion { get; set; }
    public int Strength { get; set; }
    public int Survival { get; set; }
    public int Mobility { get; set; }
    public int Focus { get; set; }
    public int Efficiency { get; set; }
    public int Resistance { get; set; }
}
