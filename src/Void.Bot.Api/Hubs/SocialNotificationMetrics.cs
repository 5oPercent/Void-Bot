using System.Diagnostics.Metrics;

namespace Void.Bot.Api.Hubs;

public static class SocialNotificationMetrics
{
    private static readonly Meter Meter;
    private static readonly UpDownCounter<long> Connections;
    private static readonly Counter<long> Notifications;
    private static readonly Counter<long> Failures;
    private static readonly Histogram<double> Delay;

    static SocialNotificationMetrics()
    {
        Meter = new Meter("Void.Bot.Social", "1.0.0");
        Connections = Meter.CreateUpDownCounter<long>("social.connections.active");
        Notifications = Meter.CreateCounter<long>("social.notifications.sent");
        Failures = Meter.CreateCounter<long>("social.notifications.failed");
        Delay = Meter.CreateHistogram<double>("social.notifications.delay", "ms");
    }

    public static void ConnectionOpened()
    {
        Connections.Add(1);
    }

    public static void ConnectionClosed()
    {
        Connections.Add(-1);
    }

    public static void NotificationSent(string surface, int connections, DateTimeOffset occurredAt)
    {
        var tag = new KeyValuePair<string, object?>("surface", surface);
        Notifications.Add(connections, tag);
        Delay.Record(Math.Max(0, (DateTimeOffset.UtcNow - occurredAt).TotalMilliseconds), tag);
    }

    public static void NotificationFailed()
    {
        Failures.Add(1);
    }
}
