using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2SeasonStatsDto
{
    public long TotalPlaytimeSeconds { get; set; }

    public bool HasData => Characters.Count > 0;

    public List<CharacterPlaytimeDto> Characters { get; set; } = [];
}

public class CharacterPlaytimeDto
{
    public string CharacterId { get; set; } = string.Empty;

    public Destiny2ClassEnum Class { get; set; } = Destiny2ClassEnum.Unknown;

    public long PlaytimeSeconds { get; set; }
}