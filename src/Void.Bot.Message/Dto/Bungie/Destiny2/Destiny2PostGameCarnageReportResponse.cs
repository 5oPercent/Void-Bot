namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2PostGameCarnageReportResponse : BungieApiResponse
{
    public Destiny2PostGameCarnageReportData? Response { get; set; }
}

public class Destiny2PostGameCarnageReportData
{
    public DateTimeOffset Period { get; set; }

    public int? StartingPhaseIndex { get; set; }

    public bool? ActivityWasStartedFromBeginning { get; set; }

    public Destiny2HistoricalStatsActivityDetailsDto ActivityDetails { get; set; } = new();

    public List<Destiny2PostGameCarnageReportEntryDto> Entries { get; set; } = [];
}

public class Destiny2PostGameCarnageReportEntryDto
{
    public Destiny2PostGameCarnageReportPlayerDto Player { get; set; } = new();

    public string CharacterId { get; set; } = string.Empty;

    public Dictionary<string, Destiny2HistoricalStatsValueDto> Values { get; set; } = [];
}

public class Destiny2PostGameCarnageReportPlayerDto
{
    public Destiny2PlayerByBungieNameDto DestinyUserInfo { get; set; } = new();
}
