using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public interface IEraArmorGenerator : IScope
{
    /// <summary>供后端正式发放流程调用；调用方必须将结果与装备及发放回执一起保存。</summary>
    EraArmorGenerationResult Generate(EraArmorGrade grade);
}

/// <summary>仅计算一次护甲生成结果，不创建资产、发放奖励或执行战斗属性效果。</summary>
public sealed class EraArmorGenerator : IEraArmorGenerator
{
    public EraArmorGenerationResult Generate(EraArmorGrade grade)
    {
        return Generate(grade, Random.Shared);
    }

    // 测试入口只向程序集内部开放，不允许客户端指定正式生成的随机数或属性。
    internal EraArmorGenerationResult Generate(EraArmorGrade grade, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var baseline = CreateBaseline(grade, random);
        var rankedValues = CreateDistribution(baseline, random);

        // 六维等价参与排列，护甲槽位不影响生成偏向。
        for (var index = rankedValues.Length - 1; index > 0; index--)
        {
            var other = random.Next(index + 1);
            (rankedValues[index], rankedValues[other]) = (rankedValues[other], rankedValues[index]);
        }

        return new EraArmorGenerationResult
        {
            Grade = grade,
            GenerationRuleVersion = EraArmorGenerationRules.Version,
            Attributes = new EraArmorAttributesDto
            {
                Strength = rankedValues[0],
                Survival = rankedValues[1],
                Mobility = rankedValues[2],
                Focus = rankedValues[3],
                Efficiency = rankedValues[4],
                Resistance = rankedValues[5]
            }
        };
    }

    internal static int[] CreateBaseline(EraArmorGrade grade, Random random)
    {
        var budget = EraArmorGenerationRules.GetBudget(grade);
        var weights = EraArmorGenerationRules.WeightsPercent;
        var baseline = weights.Select(weight => budget * weight / 100).ToArray();

        // 先随机排列再稳定按余量排序，只随机处理相同余量，不用浮点数比较。
        var order = new[] { 0, 1, 2, 3, 4, 5 };
        for (var index = order.Length - 1; index > 0; index--)
        {
            var other = random.Next(index + 1);
            (order[index], order[other]) = (order[other], order[index]);
        }

        var remaining = budget - baseline.Sum();
        foreach (var index in order.OrderByDescending(index => budget * weights[index] % 100).Take(remaining))
        {
            baseline[index]++;
        }

        return baseline;
    }

    internal static int[] CreateDistribution(int[] baseline, Random random)
    {
        var budget = baseline.Sum();
        var values = new int[6];
        var selected = new int[6];
        var validCount = 0;

        // 枚举有界整数空间并做蓄水池抽样，不依赖反复尝试碰巧生成合法结果。
        // 最后一项由剩余预算确定，六项联合浮动且始终用满预算。
        SelectDistribution(0, budget);
        if (validCount == 0)
        {
            throw new InvalidOperationException("Armor baseline has no valid distribution.");
        }

        return selected;

        void SelectDistribution(int index, int remaining)
        {
            if (index == values.Length)
            {
                if (remaining == 0 && random.Next(++validCount) == 0)
                {
                    values.CopyTo(selected, 0);
                }

                return;
            }

            var minimum = Math.Max(0, baseline[index] - EraArmorGenerationRules.MaximumDeviation);
            var maximum = Math.Min(remaining, baseline[index] + EraArmorGenerationRules.MaximumDeviation);
            if (index == values.Length - 1)
            {
                minimum = Math.Max(minimum, remaining);
            }

            for (var value = minimum; value <= maximum; value++)
            {
                if ((index is 1 or 2 && value >= values[0]) ||
                    (index >= 3 && (value >= values[1] || value >= values[2])))
                {
                    continue;
                }

                values[index] = value;
                SelectDistribution(index + 1, remaining - value);
            }
        }
    }
}

/// <summary>后端生成的单件结果；尚未持久化时不代表正式装备资产。</summary>
public sealed class EraArmorGenerationResult
{
    /// <summary>本次生成采用的护甲品阶。</summary>
    public required EraArmorGrade Grade { get; init; }

    /// <summary>与六维结果一起持久化的生成规则版本。</summary>
    public required int GenerationRuleVersion { get; init; }

    /// <summary>生成后的六维原值，不含运行时最终生效值。</summary>
    public required EraArmorAttributesDto Attributes { get; init; }
}
