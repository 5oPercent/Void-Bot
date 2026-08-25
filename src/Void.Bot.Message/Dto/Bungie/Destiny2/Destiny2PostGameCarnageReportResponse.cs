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
}
