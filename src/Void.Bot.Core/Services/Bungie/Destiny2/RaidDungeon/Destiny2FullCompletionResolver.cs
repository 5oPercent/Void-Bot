using Void.Bot.Message.Dto.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Bungie.Destiny2.RaidDungeon;

internal static class Destiny2FullCompletionResolver
{
    private static readonly DateTimeOffset BeyondLightRelease =
        new(2020, 11, 10, 0, 0, 0, TimeSpan.Zero);

    public static Destiny2FullCompletionStatus Resolve(
        bool completed,
        long timePlayedSeconds,
        long activityDurationSeconds,
        DateTimeOffset activityPeriod,
        Destiny2PgcrStartSummaryDto? pgcr,
        long playerStartThresholdSeconds,
        Destiny2PgcrPlayerSummaryDto? pgcrPlayer = null,
        long? playerStartSeconds = null)
    {
        if (!completed)
            return Destiny2FullCompletionStatus.NotCompleted;
        if (activityDurationSeconds <= 0)
            return Destiny2FullCompletionStatus.Partial;

        if (pgcr is { Available: true, ActivityWasStartedFromBeginning: false })
            return Destiny2FullCompletionStatus.Partial;

        if (pgcrPlayer is { Completed: false })
            return Destiny2FullCompletionStatus.Partial;

        var resolvedStartSeconds = pgcrPlayer?.StartSeconds ?? playerStartSeconds;
        var playerStartedFromBeginning = resolvedStartSeconds.HasValue
            ? resolvedStartSeconds.Value <= playerStartThresholdSeconds
            : timePlayedSeconds >= activityDurationSeconds;
        if (!playerStartedFromBeginning)
            return Destiny2FullCompletionStatus.Partial;

        if (pgcr is not { Available: true })
            return Destiny2FullCompletionStatus.InferredFull;

        if (pgcr.ActivityWasStartedFromBeginning == true)
            return Destiny2FullCompletionStatus.ConfirmedFull;

        var period = pgcr.Period == default ? activityPeriod : pgcr.Period;
        if (period < BeyondLightRelease && pgcr.StartingPhaseIndex.HasValue)
        {
            return pgcr.StartingPhaseIndex.Value == 0
                ? Destiny2FullCompletionStatus.ConfirmedFull
                : Destiny2FullCompletionStatus.Partial;
        }

        return Destiny2FullCompletionStatus.InferredFull;
    }

    public static bool IsFull(Destiny2FullCompletionStatus status)
    {
        return status is Destiny2FullCompletionStatus.ConfirmedFull or Destiny2FullCompletionStatus.InferredFull;
    }
}
