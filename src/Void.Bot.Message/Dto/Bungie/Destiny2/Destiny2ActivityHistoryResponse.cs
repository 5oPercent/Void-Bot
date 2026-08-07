namespace Void.Bot.Message.Dto.Bungie.Destiny2;

/// <summary>
/// Bungie API response for character activity history.
/// Used for historical season playtime aggregation.
/// </summary>
public class Destiny2ActivityHistoryResponse : BungieApiResponse
{
    public Destiny2ActivityHistoryData? Response { get; set; }
}

public class Destiny2ActivityHistoryData
{
    public List<Destiny2Activity>? Activities { get; set; }
}

public class Destiny2Activity
{
    public DateTimeOffset Period { get; set; }

    public Destiny2HistoricalStatsActivityDetailsDto ActivityDetails { get; set; } = new();

    public Dictionary<string, Destiny2HistoricalStatsValueDto> Values { get; set; } = [];
}

public class Destiny2HistoricalStatsActivityDetailsDto
{
    public uint ReferenceId { get; set; }

    public long InstanceId { get; set; }
}
