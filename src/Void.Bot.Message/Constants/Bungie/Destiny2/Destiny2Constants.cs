namespace Void.Bot.Message.Constants.Bungie.Destiny2;

public partial class Destiny2Constants
{
    public const string Destiny2 = nameof(Destiny2);

    public const string RaidDungeonStatsCachePrefix = $"{Destiny2}:RaidDungeonStats:V9";

    public const string PostGameCarnageReportCachePrefix = $"{Destiny2}:PGCR:V4";

    // Increment when persisted PGCR fields or parsing rules require historical data to be refreshed.
    public const int CurrentPgcrDataVersion = 1;

    public const uint RaidMetricParentNodeHash = 926976517;

    public const uint DungeonMetricParentNodeHash = 3707324621;
}
