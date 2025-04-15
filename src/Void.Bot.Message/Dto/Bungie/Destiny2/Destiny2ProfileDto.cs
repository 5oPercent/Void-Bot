using Void.Bot.Message.Dto.Bungie.Destiny2.Component;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

/// <summary>
/// GetProfile 接口返回结构
/// </summary>
public class Destiny2ProfileResponse : BungieApiResponse
{
    public Destiny2ProfileDataDto Response { get; set; }
}

public class Destiny2ProfileDataDto
{
    /// <summary>
    /// 基本档案
    /// </summary>
    public Destiny2ComponentProfilesDto Profiles { get; set; }
    
    /// <summary>
    /// 社交表扬
    /// </summary>
    public Destiny2ComponentSocialCommendationsDto ProfileCommendations { get; set; }
}