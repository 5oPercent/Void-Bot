namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 材料需求集合定义，描述游戏中某项操作所需的材料组合
/// </summary>
public class Destiny2ManifestMaterialRequirementSetDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 需求材料列表，包含完成特定操作所需的全部材料
    /// </summary>
    public List<Destiny2ManifestMaterialRequirementDto> Materials { get; set; } = [];
}

/// <summary>
/// 材料需求，描述特定材料的需求详情
/// </summary>
public class Destiny2ManifestMaterialRequirementDto
{
    /// <summary>
    /// 物品哈希，指向需要的具体材料物品
    /// </summary>
    public uint ItemHash { get; set; }

    /// <summary>
    /// 是否在操作完成后删除材料
    /// </summary>
    public bool DeleteOnAction { get; set; }

    /// <summary>
    /// 需要的材料数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 数量是否为常量值（不受其他因素影响）
    /// </summary>
    public bool CountIsConstant { get; set; }

    /// <summary>
    /// 是否在需求列表中省略此材料
    /// </summary>
    public bool OmitFromRequirements { get; set; }

    /// <summary>
    /// 是否拥有虚拟堆叠大小
    /// </summary>
    public bool HasVirtualStackSize { get; set; }
}

/// <summary>
/// 材料需求集合清单，存储所有材料需求集合定义的字典集合
/// </summary>
public class Destiny2ManifestMaterialRequirementSetDto : Dictionary<string, Destiny2ManifestMaterialRequirementSetDefinitionDto>
{
}