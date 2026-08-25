using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2RaidDungeonStatsDto
{
    public List<Destiny2EndgameActivityStatsDto> Activities { get; set; } = [];
}

public class Destiny2EndgameActivityStatsDto
{
    public Destiny2ActivityModeTypeEnum ActivityType { get; set; }

    public string Name { get; set; } = string.Empty;

    public string IconUrl { get; set; } = string.Empty;

    public long ActivityCount { get; set; }

    public long CompletionCount { get; set; }

    public long MentorCount { get; set; }

    public long PlaytimeSeconds { get; set; }

    public List<Destiny2EndgameActivityDetailStatsDto> Details { get; set; } = [];
}

public class Destiny2EndgameActivityDetailStatsDto
{
    public List<uint> ActivityHashes { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    public Destiny2EndgameActivityDifficultyEnum Difficulty { get; set; }

    public string IconUrl { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public long ActivityCount { get; set; }

    public long CompletionCount { get; set; }

    public long MentorCount { get; set; }

    public long PlaytimeSeconds { get; set; }
}
