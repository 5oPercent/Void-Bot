using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 平台库存映射定义，描述不同平台间物品存储位置的对应关系
/// </summary>
public class Destiny2ManifestPlatformBucketMappingDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 会员类型，指定游戏平台（如Xbox、PlayStation、Steam等）
    /// </summary>
    public BungieMembershipTypeEnum MembershipType { get; set; }
    
    /// <summary>
    /// 库存哈希，关联到物品库存定义，表示物品存储的位置分类
    /// </summary>
    public uint BucketHash { get; set; }
}

/// <summary>
/// 平台物品库存映射清单，存储所有平台物品库存映射定义的字典集合
/// </summary>
public class Destiny2ManifestPlatformBucketMappingDto : Dictionary<string, Destiny2ManifestPlatformBucketMappingDefinitionDto>
{
}