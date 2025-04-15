using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Component;

public class Destiny2ComponentSocialCommendationsDto
{
    public DestinyComponentSocialCommendationsDataDto Data { get; set; }
    
    public Destiny2ComponentPrivacyEnum Privacy { get; set; }
}

public class DestinyComponentSocialCommendationsDataDto
{
    public int TotalScore { get; set; }
    
    public Dictionary<long, long> CommendationNodePercentagesByHash { get; set; }
    
    public List<long> ScoreDetailValues { get; set; }
    
    public Dictionary<long, long> CommendationNodeScoresByHash { get; set; }
    
    public Dictionary<long, long> CommendationScoresByHash { get; set; }
    
    public Destiny2ComponentPrivacyEnum Privacy { get; set; }
}