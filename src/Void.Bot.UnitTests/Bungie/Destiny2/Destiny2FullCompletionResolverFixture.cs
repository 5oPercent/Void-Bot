using Shouldly;
using Void.Bot.Core.Services.Bungie.Destiny2.RaidDungeon;
using Void.Bot.Message.Dto.Bungie.Destiny2;

namespace Void.Bot.UnitTest.Bungie.Destiny2;

public class Destiny2FullCompletionResolverFixture
{
    private static readonly DateTimeOffset RecentActivityPeriod =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ShouldConfirmFullCompletionWhenPgcrStartedFromBeginning()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 3600, 3600, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            });

        status.ShouldBe(Destiny2FullCompletionStatus.ConfirmedFull);
    }

    [Fact]
    public void ShouldRejectCheckpointCompletionEvenWhenPlayerCoveredInstanceDuration()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 600, 600, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = false
            });

        status.ShouldBe(Destiny2FullCompletionStatus.Partial);
    }

    [Fact]
    public void ShouldRejectPlayerWhoDidNotCoverActivityDuration()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 3000, 3600, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            });

        status.ShouldBe(Destiny2FullCompletionStatus.Partial);
    }

    [Fact]
    public void ShouldRejectActivityThatWasNotCompleted()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            false, 3600, 3600, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            });

        status.ShouldBe(Destiny2FullCompletionStatus.NotCompleted);
    }

    [Theory]
    [InlineData(0, Destiny2FullCompletionStatus.ConfirmedFull)]
    [InlineData(1, Destiny2FullCompletionStatus.Partial)]
    public void ShouldUseStartingPhaseForPreBeyondLightActivities(
        int startingPhaseIndex, Destiny2FullCompletionStatus expected)
    {
        var legacyPeriod = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var status = Destiny2FullCompletionResolver.Resolve(
            true, 3600, 3600, legacyPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                Period = legacyPeriod,
                StartingPhaseIndex = startingPhaseIndex
            });

        status.ShouldBe(expected);
    }

    [Fact]
    public void ShouldIgnoreStartingPhaseForPostBeyondLightActivities()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 3600, 3600, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                Period = RecentActivityPeriod,
                StartingPhaseIndex = 1
            });

        status.ShouldBe(Destiny2FullCompletionStatus.InferredFull);
    }

    [Fact]
    public void ShouldInferFullCompletionWhenPgcrIsUnavailable()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 3600, 3600, RecentActivityPeriod, null);

        status.ShouldBe(Destiny2FullCompletionStatus.InferredFull);
        Destiny2FullCompletionResolver.IsFull(status).ShouldBeTrue();
    }
}
