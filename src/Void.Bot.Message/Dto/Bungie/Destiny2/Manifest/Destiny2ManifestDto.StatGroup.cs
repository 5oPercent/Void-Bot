namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 统计组定义
/// </summary>
public class Destiny2ManifestStatGroupDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 最大值
    /// </summary>
    public int MaximumValue { get; set; }

    /// <summary>
    /// UI位置
    /// </summary>
    public int UiPosition { get; set; }

    /// <summary>
    /// 缩放统计数据列表
    /// </summary>
    public List<Destiny2ManifestStatDisplayDefinitionDto> ScaledStats { get; set; } = [];

    /// <summary>
    /// 覆盖设置列表
    /// </summary>
    public List<object> Overrides { get; set; } = [];
}

/// <summary>
/// 统计组清单
/// </summary>
public class Destiny2ManifestStatGroupDto : Dictionary<string, Destiny2ManifestGenderDefinitionDto>
{
}