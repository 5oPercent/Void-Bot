namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 商人集合定义
/// </summary>
public class Destiny2ManifestVendorGroupDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 分类名称
    /// </summary>
    public string CategoryName { get; set; }
}

/// <summary>
/// 商人集合清单
/// </summary>
public class Destiny2ManifestVendorGroupDto : Dictionary<string, Destiny2ManifestVendorGroupDefinitionDto>
{
}