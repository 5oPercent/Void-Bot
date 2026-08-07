using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2SeasonStatsDto
{
    public long TotalPlaytimeSeconds { get; set; }

    public string TotalPlaytimeText => Destiny2PlaytimeFormatter.Format(TotalPlaytimeSeconds);

    public bool HasData => Characters.Count > 0 || Seasons.Count > 0;

    public List<CharacterPlaytimeDto> Characters { get; set; } = [];

    public List<Destiny2SeasonPlaytimeDto> Seasons { get; set; } = [];
}

public class CharacterPlaytimeDto
{
    public string CharacterId { get; set; } = string.Empty;

    public Destiny2ClassEnum Class { get; set; } = Destiny2ClassEnum.Unknown;

    public long PlaytimeSeconds { get; set; }

    public string PlaytimeText => Destiny2PlaytimeFormatter.Format(PlaytimeSeconds);
}

public class Destiny2SeasonPlaytimeDto
{
    public uint SeasonHash { get; set; }

    public int SeasonNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public long TotalPlaytimeSeconds => Classes.Sum(x => x.PlaytimeSeconds);

    public string TotalPlaytimeText => Destiny2PlaytimeFormatter.Format(TotalPlaytimeSeconds);

    public List<ClassPlaytimeDto> Classes { get; set; } = [];
}

public class ClassPlaytimeDto
{
    public Destiny2ClassEnum Class { get; set; } = Destiny2ClassEnum.Unknown;

    public long PlaytimeSeconds { get; set; }

    public string PlaytimeText => Destiny2PlaytimeFormatter.Format(PlaytimeSeconds);
}

internal static class Destiny2PlaytimeFormatter
{
    public static string Format(long seconds)
    {
        var playtime = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return $"{(long)playtime.TotalDays:D2} D {playtime.Hours:D2} H {playtime.Minutes:D2} M {playtime.Seconds:D2} S";
    }
}
