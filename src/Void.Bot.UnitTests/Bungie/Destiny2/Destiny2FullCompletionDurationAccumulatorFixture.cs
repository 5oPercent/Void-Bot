using Shouldly;
using Void.Bot.Core.Services.Bungie.Destiny2.RaidDungeon;

namespace Void.Bot.UnitTest.Bungie.Destiny2;

public class Destiny2FullCompletionDurationAccumulatorFixture
{
    [Fact]
    public void ShouldUseInferredDurationsWhenConfirmedDurationsAreUnavailable()
    {
        var accumulator = new Destiny2FullCompletionDurationAccumulator();

        accumulator.Add(Destiny2FullCompletionStatus.InferredFull, 600);
        accumulator.Add(Destiny2FullCompletionStatus.InferredFull, 900);

        accumulator.FastestSeconds.ShouldBe(600);
        accumulator.AverageSeconds.ShouldBe(750d);
    }

    [Fact]
    public void ShouldPreferConfirmedDurationsOverFasterInferredDurations()
    {
        var accumulator = new Destiny2FullCompletionDurationAccumulator();

        accumulator.Add(Destiny2FullCompletionStatus.InferredFull, 600);
        accumulator.Add(Destiny2FullCompletionStatus.ConfirmedFull, 3600);
        accumulator.Add(Destiny2FullCompletionStatus.ConfirmedFull, 4200);

        accumulator.FastestSeconds.ShouldBe(3600);
        accumulator.AverageSeconds.ShouldBe(3900d);
    }
}
