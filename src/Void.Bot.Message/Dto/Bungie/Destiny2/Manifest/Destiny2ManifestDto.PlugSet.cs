namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 插件集定义，描述可用于物品插槽的插件集合
/// </summary>
public class Destiny2ManifestPlugSetDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 可重用插件物品列表，可用于物品插槽的插件集合
    /// </summary>
    public List<Destiny2ManifestItemSocketEntryPlugItemRandomizedDefinitionDto> ReusablePlugItems { get; set; } = [];

    /// <summary>
    /// 是否为虚拟插件集，表示这个插件集是否为系统生成的临时集合
    /// </summary>
    public bool IsFakePlugSet { get; set; }
}

/// <summary>
/// 物品插槽条目插件物品随机化定义，描述可随机获取的插件物品及其概率
/// </summary>
public class Destiny2ManifestItemSocketEntryPlugItemRandomizedDefinitionDto : Destiny2ManifestItemSocketEntryPlugItemDefinitionDto
{
    /// <summary>
    /// 制作需求，描述制作该插件所需的材料和条件
    /// </summary>
    public Destiny2ManifestPlugItemCraftingRequirementsDto CraftingRequirements { get; set; } = new();

    /// <summary>
    /// 权重，决定在随机选择中的出现概率
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// 备选权重，在特定条件下的替代概率权重
    /// </summary>
    public int AlternateWeight { get; set; }

    /// <summary>
    /// 当前是否可随机获取，表示该插件是否在当前条件下可被随机选中
    /// </summary>
    public bool CurrentlyCanRoll { get; set; }
}

/// <summary>
/// 插件物品制作需求，描述制作插件所需的条件和材料
/// </summary>
public class Destiny2ManifestPlugItemCraftingRequirementsDto
{
    /// <summary>
    /// 解锁需求列表，描述解锁该插件制作能力所需满足的条件
    /// </summary>
    public List<Destiny2ManifestPlugItemCraftingUnlockRequirementDto> UnlockRequirements { get; set; } = [];

    /// <summary>
    /// 所需级别，制作该插件所需的最低制作等级
    /// </summary>
    public int RequiredLevel { get; set; }

    /// <summary>
    /// 材料需求哈希列表，关联到制作所需的各种材料定义
    /// </summary>
    public List<uint> MaterialRequirementHashes { get; set; } = [];
}

/// <summary>
/// 插件物品制作解锁需求，描述解锁插件制作能力的特定条件
/// </summary>
public class Destiny2ManifestPlugItemCraftingUnlockRequirementDto
{
    /// <summary>
    /// 失败描述，当无法满足解锁条件时显示的文本
    /// </summary>
    public string FailureDescription { get; set; } = string.Empty;
}

/// <summary>
/// 插件集清单，存储所有插件集定义的字典集合
/// </summary>
public class Destiny2ManifestPlugSetDto : Dictionary<string, Destiny2ManifestPlugSetDefinitionDto>
{
}