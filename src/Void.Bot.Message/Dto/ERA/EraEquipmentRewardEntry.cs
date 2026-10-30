using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>引用已有装备模板的奖励条目；抽池结果和业务直接发放共用，不是装备定义。</summary>
public class EraEquipmentRewardEntry
{
    public Guid TemplateId { get; set; }
    public EraEquipmentGrade Grade { get; set; } = EraEquipmentGrade.Common;
    public int Count { get; set; } = 1;
}
