namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2权限优惠定义，描述可获得的特定权限优惠内容
/// </summary>
public class Destiny2ManifestEntitlementOfferDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 优惠密钥，唯一标识特定的权限优惠
    /// </summary>
    public string OfferKey { get; set; } = string.Empty;
}

/// <summary>
/// 命运2权限优惠清单，包含所有可用的权限优惠定义
/// </summary>
public class Destiny2ManifestEntitlementOfferDto : Dictionary<string, Destiny2ManifestEntitlementOfferDefinitionDto>
{
}