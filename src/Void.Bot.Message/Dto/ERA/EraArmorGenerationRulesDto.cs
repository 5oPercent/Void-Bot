using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>生成规则说明；客户端不据此重新生成正式属性。</summary>
public sealed class EraArmorGenerationRulesDto
{
    /// <summary>保存到护甲实例上的生成规则版本。</summary>
    public int Version { get; set; }
    /// <summary>主、次、剩余六个分配位置的百分比。</summary>
    public List<int> WeightsPercent { get; set; } = new List<int>();
    /// <summary>各位置相对整数基准的最大点数变化。</summary>
    public int MaximumDeviation { get; set; }
    /// <summary>已确认的品阶及预算，不表示掉落概率。</summary>
    public List<EraArmorGradeBudgetDto> GradeBudgets { get; set; } = new List<EraArmorGradeBudgetDto>();
}
