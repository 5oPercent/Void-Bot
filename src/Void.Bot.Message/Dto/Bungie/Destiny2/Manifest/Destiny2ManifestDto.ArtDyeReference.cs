namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 艺术染色引用定义
/// </summary>
public class Destiny2ManifestArtDyeReferenceDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 艺术染色哈希
    /// </summary>
    public uint ArtDyeHash { get; set; }

    /// <summary>
    /// 染色清单哈希
    /// </summary>
    public uint DyeManifestHash { get; set; }
}

/// <summary>
/// 艺术染色引用清单，包含所有染色引用的字典集合
/// </summary>
public class Destiny2ManifestArtDyeReferenceDto : Dictionary<string, Destiny2ManifestArtDyeReferenceDefinitionDto>
{
}