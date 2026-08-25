namespace Void.Bot.Core.Services.Bungie.Destiny2.RaidDungeon;

internal sealed class Destiny2FullCompletionDurationAccumulator
{
    private long _confirmedTotalSeconds;
    private long _confirmedCount;
    private long? _confirmedFastestSeconds;
    private long _inferredTotalSeconds;
    private long _inferredCount;
    private long? _inferredFastestSeconds;

    public long? FastestSeconds => _confirmedCount > 0
        ? _confirmedFastestSeconds
        : _inferredFastestSeconds;

    public double? AverageSeconds => _confirmedCount > 0
        ? (double)_confirmedTotalSeconds / _confirmedCount
        : _inferredCount > 0
            ? (double)_inferredTotalSeconds / _inferredCount
            : null;

    public void Add(Destiny2FullCompletionStatus status, long durationSeconds)
    {
        if (durationSeconds <= 0)
            return;

        if (status == Destiny2FullCompletionStatus.ConfirmedFull)
        {
            _confirmedTotalSeconds += durationSeconds;
            _confirmedCount++;
            if (_confirmedFastestSeconds == null || durationSeconds < _confirmedFastestSeconds)
                _confirmedFastestSeconds = durationSeconds;
        }
        else if (status == Destiny2FullCompletionStatus.InferredFull)
        {
            _inferredTotalSeconds += durationSeconds;
            _inferredCount++;
            if (_inferredFastestSeconds == null || durationSeconds < _inferredFastestSeconds)
                _inferredFastestSeconds = durationSeconds;
        }
    }
}
