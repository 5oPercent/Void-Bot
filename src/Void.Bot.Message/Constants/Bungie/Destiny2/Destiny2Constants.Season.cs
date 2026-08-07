namespace Void.Bot.Message.Constants.Bungie.Destiny2;

public partial class Destiny2Constants
{
    public static readonly IReadOnlyDictionary<int, (DateTime StartDate, DateTime EndDate)> LegacySeasonDateRanges =
        new Dictionary<int, (DateTime StartDate, DateTime EndDate)>
        {
            [1] = (new DateTime(2017, 9, 6, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 12, 5, 17, 0, 0, DateTimeKind.Utc)),
            [2] = (new DateTime(2017, 12, 5, 17, 0, 0, DateTimeKind.Utc), new DateTime(2018, 5, 8, 17, 0, 0, DateTimeKind.Utc)),
            [3] = (new DateTime(2018, 5, 8, 17, 0, 0, DateTimeKind.Utc), new DateTime(2018, 9, 4, 17, 0, 0, DateTimeKind.Utc)),
            [4] = (new DateTime(2018, 9, 4, 17, 0, 0, DateTimeKind.Utc), new DateTime(2018, 12, 4, 17, 0, 0, DateTimeKind.Utc)),
            [5] = (new DateTime(2018, 12, 4, 17, 0, 0, DateTimeKind.Utc), new DateTime(2019, 3, 5, 17, 0, 0, DateTimeKind.Utc)),
            [6] = (new DateTime(2019, 3, 5, 17, 0, 0, DateTimeKind.Utc), new DateTime(2019, 6, 4, 17, 0, 0, DateTimeKind.Utc)),
            [7] = (new DateTime(2019, 6, 4, 17, 0, 0, DateTimeKind.Utc), new DateTime(2019, 10, 1, 17, 0, 0, DateTimeKind.Utc))
        };
}
