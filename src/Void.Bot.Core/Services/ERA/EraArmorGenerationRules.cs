using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

/// <summary>首版护甲生成规则；已有装备只读取保存结果，不随此规则变化重新生成。</summary>
public static class EraArmorGenerationRules
{
    /// <summary>生成规则版本，不是配置发布版本、资产并发版本或缓存版本。</summary>
    public const int Version = 1;

    /// <summary>每个分配位置相对整数基准允许的最大点数变化。</summary>
    public const int MaximumDeviation = 3;

    /// <summary>六个主次分配位置的百分比，生成与目录说明共用。</summary>
    public static IReadOnlyList<int> WeightsPercent { get; } = Array.AsReadOnly(new[] { 40, 20, 15, 10, 10, 5 });

    /// <summary>读取已确认品阶的单件预算；未定义品阶不回退到默认值。</summary>
    public static int GetBudget(EraArmorGrade grade)
    {
        return grade switch
        {
            EraArmorGrade.Worn => 54,
            EraArmorGrade.Common => 60,
            EraArmorGrade.Reliable => 69,
            EraArmorGrade.Unusual => 75,
            EraArmorGrade.Unique => 81,
            _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, "Unknown armor grade.")
        };
    }
}
