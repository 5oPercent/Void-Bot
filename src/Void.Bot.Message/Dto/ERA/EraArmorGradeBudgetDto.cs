using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>一个品阶的生成预算。</summary>
public sealed class EraArmorGradeBudgetDto
{
    /// <summary>品阶协议值。</summary>
    public EraEquipmentGrade Grade { get; set; }
    /// <summary>单件六维合计。</summary>
    public int Budget { get; set; }
}
