namespace Void.Bot.IntegrationTest.Services.ERA;

/// <summary>只控制业务 UTC 时间，不等待真实秒数；SQL 与 Redis 仍使用真实连接。</summary>
internal sealed class EraWorldTestClock : TimeProvider
{
    private long _ticks;

    public EraWorldTestClock(DateTimeOffset now)
    {
        _ticks = now.UtcTicks - now.UtcTicks % 10;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return new DateTimeOffset(Interlocked.Read(ref _ticks), TimeSpan.Zero);
    }

    public void Advance(TimeSpan elapsed)
    {
        Interlocked.Add(ref _ticks, elapsed.Ticks);
    }
}
