using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Services.Bungie.Destiny2.RaidDungeon;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Dto.Bungie.Destiny2;

namespace Void.Bot.UnitTest.Bungie.Destiny2;

public class Destiny2FullCompletionResolverFixture
{
    private static readonly Destiny2Setting Setting =
        new(new ConfigurationBuilder().Build());

    private static int PlayerStartThresholdSeconds =>
        Setting.RaidDungeonPlayerStartThresholdSeconds;

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
            }, PlayerStartThresholdSeconds);

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
            }, PlayerStartThresholdSeconds);

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
            }, PlayerStartThresholdSeconds);

        status.ShouldBe(Destiny2FullCompletionStatus.Partial);
    }

    [Fact]
    public void ShouldAllowSmallPlayerStartOffsetForFullCompletion()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 1495, 1497, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            },
            PlayerStartThresholdSeconds,
            new Destiny2PgcrPlayerSummaryDto
            {
                Completed = true,
                StartSeconds = 2
            });

        status.ShouldBe(Destiny2FullCompletionStatus.ConfirmedFull);
    }

    [Fact]
    public void ShouldRejectPlayerWhoStartedAfterTolerance()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 1437, 1497, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            },
            PlayerStartThresholdSeconds,
            new Destiny2PgcrPlayerSummaryDto
            {
                Completed = true,
                StartSeconds = PlayerStartThresholdSeconds + 1
            });

        status.ShouldBe(Destiny2FullCompletionStatus.Partial);
    }

    [Fact]
    public void ShouldUseConfiguredPlayerStartThreshold()
    {
        var configuredThreshold = PlayerStartThresholdSeconds + 30;
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 1437, 1497, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            },
            configuredThreshold,
            new Destiny2PgcrPlayerSummaryDto
            {
                Completed = true,
                StartSeconds = configuredThreshold
            });

        status.ShouldBe(Destiny2FullCompletionStatus.ConfirmedFull);
    }

    [Fact]
    public void ShouldUseActivityHistoryStartOffsetWhenPgcrPlayerIsMissing()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 1495, 1497, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            },
            PlayerStartThresholdSeconds,
            playerStartSeconds: 2);

        status.ShouldBe(Destiny2FullCompletionStatus.ConfirmedFull);
    }

    [Fact]
    public void ShouldRejectPlayerMarkedIncompleteByPgcr()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 1497, 1497, RecentActivityPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                ActivityWasStartedFromBeginning = true
            },
            PlayerStartThresholdSeconds,
            new Destiny2PgcrPlayerSummaryDto
            {
                Completed = false,
                StartSeconds = 0
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
            }, PlayerStartThresholdSeconds);

        status.ShouldBe(Destiny2FullCompletionStatus.NotCompleted);
    }

    [Theory]
    [InlineData(0, (int)Destiny2FullCompletionStatus.ConfirmedFull)]
    [InlineData(1, (int)Destiny2FullCompletionStatus.Partial)]
    public void ShouldUseStartingPhaseForPreBeyondLightActivities(
        int startingPhaseIndex, int expected)
    {
        var legacyPeriod = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var status = Destiny2FullCompletionResolver.Resolve(
            true, 3600, 3600, legacyPeriod,
            new Destiny2PgcrStartSummaryDto
            {
                Available = true,
                Period = legacyPeriod,
                StartingPhaseIndex = startingPhaseIndex
            }, PlayerStartThresholdSeconds);

        status.ShouldBe((Destiny2FullCompletionStatus)expected);
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
            }, PlayerStartThresholdSeconds);

        status.ShouldBe(Destiny2FullCompletionStatus.InferredFull);
    }

    [Fact]
    public void ShouldInferFullCompletionWhenPgcrIsUnavailable()
    {
        var status = Destiny2FullCompletionResolver.Resolve(
            true, 3600, 3600, RecentActivityPeriod, null, PlayerStartThresholdSeconds);

        status.ShouldBe(Destiny2FullCompletionStatus.InferredFull);
        Destiny2FullCompletionResolver.IsFull(status).ShouldBeTrue();
    }
}
