using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2PgcrStartSummaryDto
{
    public bool Available { get; set; }

    public int DataVersion { get; set; }

    public DateTimeOffset Period { get; set; }

    public uint ReferenceId { get; set; }

    public int? StartingPhaseIndex { get; set; }

    public bool? ActivityWasStartedFromBeginning { get; set; }

    public Dictionary<string, Destiny2PgcrPlayerSummaryDto> Players { get; set; } = [];
}

public class Destiny2PgcrPlayerSummaryDto
{
    public BungieMembershipTypeEnum? MembershipType { get; set; }

    public string? MembershipId { get; set; }

    public bool? Completed { get; set; }

    public long? StartSeconds { get; set; }
}
