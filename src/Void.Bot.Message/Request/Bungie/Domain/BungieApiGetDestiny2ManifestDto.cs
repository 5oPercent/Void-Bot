using Void.Bot.Message.Dto.Bungie;

namespace Void.Bot.Message.Request.Bungie.Domain;

public class BungieApiGetDestiny2ManifestResponse : BungieApiResponse
{
    public BungieApiGetDestiny2ManifestDto Response { get; set; }
}

/// <summary>
/// 命运2内容清单, 已省略Mobile相关字段.
/// </summary>
public class BungieApiGetDestiny2ManifestDto
{
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 完整内容数据清单, 200~300M的json.
    /// </summary>
    public BungieLanguageContentDto JsonWorldContentPaths { get; set; } = new();
    
    /// <summary>
    /// 各个组件资源清单路径, 可根据不同组件请求专门的json数据内容.
    /// </summary>
    public BungieLanguageComponentContentDto JsonWorldComponentContentPaths { get; set; } = new();
}