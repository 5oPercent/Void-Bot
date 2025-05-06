namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 进度级别需求定义，描述不同进度等级所需的要求和对应数值
/// </summary>
public class Destiny2ManifestProgressionLevelRequirementDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 插值点列表，定义不同进度等级对应的数值变化曲线
    /// </summary>
    public List<Destiny2ManifestInterpolationPointDefinitionDto<float>> InterpolationPoints { get; set; } = [];
    
    /// <summary>
    /// 进度系统哈希，关联到此需求对应的进度系统定义
    /// </summary>
    public uint ProgressionHash { get; set; }
}

/// <summary>
/// 进度级别需求清单，存储所有进度级别需求定义的字典集合
/// </summary>
public class Destiny2ManifestProgressionLevelDto : Dictionary<string, Destiny2ManifestProgressionLevelRequirementDto>
{
}