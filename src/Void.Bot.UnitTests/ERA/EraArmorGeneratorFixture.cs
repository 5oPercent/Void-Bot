using Shouldly;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraArmorGeneratorFixture
{
    [Theory]
    [InlineData(EraArmorGrade.Worn, 54, new[] { 22, 11, 8, 5, 5, 3 })]
    [InlineData(EraArmorGrade.Common, 60, new[] { 24, 12, 9, 6, 6, 3 })]
    [InlineData(EraArmorGrade.Reliable, 69, new[] { 28, 14, 10, 7, 7, 3 })]
    [InlineData(EraArmorGrade.Unusual, 75, new[] { 30, 15, 11, 8, 7, 4 })]
    [InlineData(EraArmorGrade.Unique, 81, new[] { 33, 16, 12, 8, 8, 4 })]
    public void Should_allocate_entire_budget_by_largest_remainder(
        EraArmorGrade grade,
        int budget,
        int[] expected)
    {
        EraArmorGenerationRules.GetBudget(grade).ShouldBe(budget);
        var baseline = EraArmorGenerator.CreateBaseline(grade, new MaximumRandom());

        baseline.ShouldBe(expected);
        baseline.Sum().ShouldBe(budget);
    }

    [Fact]
    public void Should_allow_either_tied_remainder_without_changing_higher_priorities()
    {
        var first = EraArmorGenerator.CreateBaseline(EraArmorGrade.Unusual, new MaximumRandom());
        var second = EraArmorGenerator.CreateBaseline(EraArmorGrade.Unusual, new SwappedTieRandom());

        first.ShouldBe(new[] { 30, 15, 11, 8, 7, 4 });
        second.ShouldBe(new[] { 30, 15, 11, 7, 8, 4 });
    }

    [Theory]
    [InlineData(EraArmorGrade.Worn)]
    [InlineData(EraArmorGrade.Common)]
    [InlineData(EraArmorGrade.Reliable)]
    [InlineData(EraArmorGrade.Unusual)]
    [InlineData(EraArmorGrade.Unique)]
    public void Should_preserve_budget_deviation_nonnegative_values_and_rank_hierarchy(EraArmorGrade grade)
    {
        for (var seed = 0; seed < 32; seed++)
        {
            var random = new Random(seed);
            var baseline = EraArmorGenerator.CreateBaseline(grade, random);
            var values = EraArmorGenerator.CreateDistribution(baseline, random);

            values.Sum().ShouldBe(EraArmorGenerationRules.GetBudget(grade));
            for (var index = 0; index < values.Length; index++)
            {
                values[index].ShouldBeGreaterThanOrEqualTo(0);
                Math.Abs(values[index] - baseline[index]).ShouldBeLessThanOrEqualTo(3);
            }

            values[0].ShouldBeGreaterThan(values[1]);
            values[0].ShouldBeGreaterThan(values[2]);
            foreach (var value in values.Skip(3))
            {
                values[1].ShouldBeGreaterThan(value);
                values[2].ShouldBeGreaterThan(value);
            }
        }
    }

    [Fact]
    public void Should_support_zero_and_full_deviation_without_probabilistic_retries()
    {
        var baseline = new[] { 24, 12, 9, 6, 6, 3 };
        var values = EraArmorGenerator.CreateDistribution(baseline, new ZeroRandom());

        values.Sum().ShouldBe(60);
        values[0].ShouldBe(27);
        values[5].ShouldBe(0);
        values.All(value => value >= 0).ShouldBeTrue();
    }

    [Theory]
    [InlineData(EraArmorGrade.Worn)]
    [InlineData(EraArmorGrade.Common)]
    [InlineData(EraArmorGrade.Reliable)]
    [InlineData(EraArmorGrade.Unusual)]
    [InlineData(EraArmorGrade.Unique)]
    public void Should_return_versioned_final_attributes_with_reproducible_test_input(EraArmorGrade grade)
    {
        var generator = new EraArmorGenerator();
        var first = generator.Generate(grade, new Random(17));
        var second = generator.Generate(grade, new Random(17));

        first.Grade.ShouldBe(grade);
        first.GenerationRuleVersion.ShouldBe(1);
        Values(first.Attributes).ShouldBe(Values(second.Attributes));
        Values(first.Attributes).Sum().ShouldBe(EraArmorGenerationRules.GetBudget(grade));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(6)]
    public void Should_reject_undefined_grade(int grade)
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new EraArmorGenerator().Generate((EraArmorGrade)grade));
    }

    [Fact]
    public void Should_preserve_previous_result_after_another_generation()
    {
        var generator = new EraArmorGenerator();
        var first = generator.Generate(EraArmorGrade.Common, new Random(17));
        var saved = Values(first.Attributes);

        generator.Generate(EraArmorGrade.Unique, new Random(33));

        Values(first.Attributes).ShouldBe(saved);
    }

    [Fact]
    public void Should_allow_all_six_attributes_to_be_primary()
    {
        var generator = new EraArmorGenerator();
        // 只控制最后的排列，让每一维轮流拿到主属性；不靠随机种子碰巧覆盖六种结果。
        for (var target = 0; target < 6; target++)
        {
            var result = generator.Generate(EraArmorGrade.Common, new PrimaryAttributeRandom(target));
            var values = Values(result.Attributes);

            values[target].ShouldBe(values.Max());
            values.Count(value => value == values.Max()).ShouldBe(1);
        }
    }

    [Fact]
    public void Should_allow_common_armor_to_reach_two_full_attributes()
    {
        var generator = new EraArmorGenerator();
        var first = generator.Generate(EraArmorGrade.Common, new PrimaryAttributeRandom(0));
        var second = generator.Generate(EraArmorGrade.Common, new PrimaryAttributeRandom(1));

        // 三件 A 主 B 次、三件 B 主 A 次；允许保留超过最终生效上限的原始贡献。
        var strength = 3 * (first.Attributes.Strength + second.Attributes.Strength);
        var survival = 3 * (first.Attributes.Survival + second.Attributes.Survival);
        strength.ShouldBeGreaterThanOrEqualTo(100);
        survival.ShouldBeGreaterThanOrEqualTo(100);
    }

    private static int[] Values(EraArmorAttributesDto attributes)
    {
        return new[]
        {
            attributes.Strength, attributes.Survival, attributes.Mobility,
            attributes.Focus, attributes.Efficiency, attributes.Resistance
        };
    }

    private sealed class MaximumRandom : Random
    {
        public override int Next(int maxValue)
        {
            return maxValue - 1;
        }
    }

    private sealed class SwappedTieRandom : Random
    {
        public override int Next(int maxValue)
        {
            return maxValue == 5 ? 3 : maxValue - 1;
        }
    }

    private sealed class ZeroRandom : Random
    {
        public override int Next(int maxValue)
        {
            return 0;
        }
    }

    private sealed class PrimaryAttributeRandom(int target) : Random
    {
        private int _calls;
        private int _previousMaximum;
        private bool _permuting;

        public override int Next(int maxValue)
        {
            _calls++;
            if (_calls <= 5)
            {
                return maxValue - 1;
            }

            // 分配阶段的蓄水池上界单调递增，回落表示开始最终排列。
            if (maxValue < _previousMaximum)
            {
                _permuting = true;
            }

            _previousMaximum = maxValue;
            // 固定选择最后一个合法分配，在排列阶段只交换指定维度与主属性。
            return !_permuting || maxValue == target + 1 ? 0 : maxValue - 1;
        }
    }
}
