namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2PgcrStartSummaryDto
{
    public bool Available { get; set; }

    public DateTimeOffset Period { get; set; }

    public int? StartingPhaseIndex { get; set; }

    public bool? ActivityWasStartedFromBeginning { get; set; }
}
