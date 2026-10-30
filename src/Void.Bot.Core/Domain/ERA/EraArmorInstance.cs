using Void.Bot.Message.Dto.ERA;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>一件护甲唯一的最终生成结果，不存放主次排序、临时 BUFF 或运行时生效值。</summary>
[Table("era_armor_instance")]
public sealed class EraArmorInstance : IEntityBase
{
    [Key]
    public Guid EquipmentInstanceId { get; set; }
    /// <summary>读取关系表后组装的固定候选与当前选择，不在六维表内重复保存。</summary>
    [NotMapped] public EraArmorInstanceDto Modifications { get; set; } = new();
    public int GenerationRuleVersion { get; set; }
    public int Strength { get; set; }
    public int Survival { get; set; }
    public int Mobility { get; set; }
    public int Focus { get; set; }
    public int Efficiency { get; set; }
    public int Resistance { get; set; }
}
