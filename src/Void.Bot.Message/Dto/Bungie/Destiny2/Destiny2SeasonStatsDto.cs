using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2SeasonStatsDto
{
    public long TotalPlaytimeSeconds { get; set; }

    public bool HasData => Characters.Count > 0 || Seasons.Count > 0;

    public List<CharacterPlaytimeDto> Characters { get; set; } = [];

    public List<Destiny2SeasonPlaytimeDto> Seasons { get; set; } = [];
}

public class CharacterPlaytimeDto
{
    public string CharacterId { get; set; } = string.Empty;

    public Destiny2ClassEnum Class { get; set; } = Destiny2ClassEnum.Unknown;

    public long PlaytimeSeconds { get; set; }
}

public class Destiny2SeasonPlaytimeDto
{
    public uint SeasonHash { get; set; }

    public int SeasonNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public long TotalPlaytimeSeconds => Classes.Sum(x => x.PlaytimeSeconds);

    public List<ClassPlaytimeDto> Classes { get; set; } = [];
}

public class ClassPlaytimeDto
{
    public Destiny2ClassEnum Class { get; set; } = Destiny2ClassEnum.Unknown;

    public long PlaytimeSeconds { get; set; }
}
