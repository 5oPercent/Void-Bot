namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 艺术染色通道定义
/// </summary>
public class Destiny2ManifestArtDyeChannelDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 通道哈希值
    /// </summary>
    public uint ChannelHash { get; set; }
}

/// <summary>
/// 艺术染色通道清单，包含所有通道的字典集合
/// </summary>
public class Destiny2ManifestArtDyeChannelDto : Dictionary<string, Destiny2ManifestArtDyeChannelDefinitionDto>
{
}