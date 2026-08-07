namespace Void.Bot.Message.Dto.Bungie.Destiny2;

/// <summary>
/// Bungie Stats API response wrapper
/// </summary>
public class Destiny2StatsResponse : BungieApiResponse
{
    public Destiny2HistoricalStatsAccountResultDto Response { get; set; } = new();
}

public class Destiny2HistoricalStatsAccountResultDto
{
    public Destiny2HistoricalStatsWithMergedDto MergedDeletedCharacters { get; set; } = new();

    public Destiny2HistoricalStatsWithMergedDto MergedAllCharacters { get; set; } = new();

    public List<Destiny2HistoricalStatsPerCharacterDto> Characters { get; set; } = [];
}

public class Destiny2HistoricalStatsWithMergedDto
{
    public Destiny2HistoricalStatsByPeriodDto Results { get; set; } = new();

    public Destiny2HistoricalStatsByPeriodDto Merged { get; set; } = new();
}

public class Destiny2HistoricalStatsPerCharacterDto
{
    public string CharacterId { get; set; } = string.Empty;

    public bool Deleted { get; set; }

    public Destiny2HistoricalStatsByPeriodDto? Merged { get; set; }
}

public class Destiny2HistoricalStatsByPeriodDto
{
    public Destiny2AllTimeStatsDto? AllTime { get; set; }
}

// TODO: 待补全所有统计字段
public class Destiny2AllTimeStatsDto
{
    public Destiny2HistoricalStatsValueDto? SecondsPlayed { get; set; }

    public Destiny2HistoricalStatsValueDto? ActivitiesCleared { get; set; }
}

public class Destiny2HistoricalStatsValueDto
{
    public string StatId { get; set; } = string.Empty;

    public Destiny2HistoricalStatsValuePairDto? Basic { get; set; }

    public Destiny2HistoricalStatsValuePairDto? Pga { get; set; }
}

public class Destiny2HistoricalStatsValuePairDto
{
    public double Value { get; set; }

    public string DisplayValue { get; set; } = string.Empty;
}