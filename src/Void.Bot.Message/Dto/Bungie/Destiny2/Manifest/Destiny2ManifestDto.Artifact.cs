namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2神器定义，描述季节性神器及其升级系统
/// </summary>
public class Destiny2ManifestArtifactDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含神器的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 翻译块，包含神器的本地化文本信息
    /// </summary>
    public Destiny2ManifestItemTranslationBlockDefinitionDto TranslationBlock { get; set; } = new();

    /// <summary>
    /// 层级列表，包含神器的所有解锁层级
    /// </summary>
    public List<Destiny2ManifestArtifactTierDefinitionDto> Tiers { get; set; } = [];
}

/// <summary>
/// 命运2神器层级定义，描述神器中的单个解锁层级
/// </summary>
public class Destiny2ManifestArtifactTierDefinitionDto
{
    /// <summary>
    /// 层级哈希，用于唯一标识此神器层级
    /// </summary>
    public uint TierHash { get; set; }

    /// <summary>
    /// 显示标题，层级在界面中显示的名称
    /// </summary>
    public string DisplayTitle { get; set; } = string.Empty;

    /// <summary>
    /// 进度需求信息，描述解锁此层级所需的条件
    /// </summary>
    public string ProgressRequirementMessage { get; set; } = string.Empty;

    /// <summary>
    /// 物品列表，此层级包含的所有可解锁Mod或能力
    /// </summary>
    public List<Destiny2ManifestArtifactTierItemDefinitionDto> Items { get; set; } = [];

    /// <summary>
    /// 最低解锁点数需求，解锁此层级所需使用的最少能力点数
    /// </summary>
    public uint MinimumUnlockPointsUsedRequirement { get; set; }
}

/// <summary>
/// 命运2神器层级物品定义，描述层级中的单个可解锁物品
/// </summary>
public class Destiny2ManifestArtifactTierItemDefinitionDto
{
    /// <summary>
    /// 物品哈希，关联到可解锁的具体Mod或能力
    /// </summary>
    public uint ItemHash { get; set; }
}

/// <summary>
/// 命运2神器清单，包含所有神器的定义集合
/// </summary>
public class Destiny2ManifestArtifactDto : Dictionary<string, Destiny2ManifestArtifactDefinitionDto>
{
}