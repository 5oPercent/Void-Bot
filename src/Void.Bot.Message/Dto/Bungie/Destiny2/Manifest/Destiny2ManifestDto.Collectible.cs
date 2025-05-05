using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 收藏品定义
/// </summary>
public class Destiny2ManifestCollectibleDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 范围
    /// </summary>
    public Destiny2ScopeEnum Scope { get; set; }

    /// <summary>
    /// 来源字符串
    /// </summary>
    public string SourceString { get; set; } = string.Empty;

    /// <summary>
    /// 来源哈希
    /// </summary>
    public uint SourceHash { get; set; }
    
    /// <summary>
    /// 物品哈希
    /// </summary>
    public uint ItemHash { get; set; }

    /// <summary>
    /// 获取信息
    /// </summary>
    public Destiny2ManifestCollectibleAcquisitionBlockDto AcquisitionInfo { get; set; } = new();
    
    /// <summary>
    /// 状态信息
    /// </summary>
    public Destiny2ManifestCollectibleStateBlockDto StateInfo { get; set; } = new();
    
    /// <summary>
    /// 展示信息
    /// </summary>
    public Destiny2ManifestPresentationChildBlockDefinitionDto PresentationInfo { get; set; } = new();

    /// <summary>
    /// 展示节点类型
    /// </summary>
    public Destiny2PresentationNodeTypeEnum PresentationNodeType { get; set; }

    /// <summary>
    /// 特性ID列表
    /// </summary>
    public List<string> TraitIds { get; set; } = [];
    
    /// <summary>
    /// 特性哈希列表
    /// </summary>
    public List<uint> TraitHashes { get; set; } = [];
    
    /// <summary>
    /// 父节点哈希列表
    /// </summary>
    public List<uint> ParentNodeHashes { get; set; } = [];
}

/// <summary>
/// 命运2清单收藏品获取定义，描述如何获取收藏品的信息
/// </summary>
public class Destiny2ManifestCollectibleAcquisitionBlockDto
{
    /// <summary>
    /// 获取所需材料哈希
    /// </summary>
    public uint AcquireMaterialRequirementHash { get; set; }
    
    /// <summary>
    /// 获取时间戳解锁值哈希
    /// </summary>
    public uint AcquireTimestampUnlockValueHash { get; set; }
}

/// <summary>
/// 命运2清单收藏品状态定义，描述收藏品的状态信息
/// </summary>
public class Destiny2ManifestCollectibleStateBlockDto
{
    /// <summary>
    /// 被遮蔽的覆盖物品哈希
    /// </summary>
    public uint ObscuredOverrideItemHash { get; set; }
    
    /// <summary>
    /// 需求
    /// </summary>
    public Destiny2ManifestPresentationNodeRequirementsBlockDefinitionDto Requirements { get; set; } = new();
}

/// <summary>
/// 收藏品清单
/// </summary>
public class Destiny2ManifestCollectibleDto : Dictionary<string, Destiny2ManifestCollectibleDefinitionDto>
{
}