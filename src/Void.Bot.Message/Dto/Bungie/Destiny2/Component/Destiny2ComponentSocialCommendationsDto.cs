using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Component;

public class Destiny2ComponentSocialCommendationsDto
{
    public DestinyComponentSocialCommendationsDataDto Data { get; set; } = new();
    
    public Destiny2ComponentPrivacyEnum Privacy { get; set; }
}

public class DestinyComponentSocialCommendationsDataDto
{
    public int TotalScore { get; set; }

    public Dictionary<long, long> CommendationNodePercentagesByHash { get; set; } = new();

    public List<long> ScoreDetailValues { get; set; } = [];

    public Dictionary<long, long> CommendationNodeScoresByHash { get; set; } = new();
    
    public Dictionary<long, long> CommendationScoresByHash { get; set; } = new();
    
    public Destiny2ComponentPrivacyEnum Privacy { get; set; }
}