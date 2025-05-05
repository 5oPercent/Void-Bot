namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2举报原因分类定义，描述游戏中各种举报类型的分类
/// </summary>
public class Destiny2ManifestReportReasonCategoryDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含分类的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 举报原因字典，包含此分类下所有可选的具体举报原因
    /// </summary>
    public Dictionary<uint, Destiny2ManifestReportReasonDefinitionDto> Reasons { get; set; } = new();
}

/// <summary>
/// 命运2举报原因定义，描述具体举报原因及其详情
/// </summary>
public class Destiny2ManifestReportReasonDefinitionDto
{
    /// <summary>
    /// 原因哈希，用于唯一标识此举报原因
    /// </summary>
    public uint ReasonHash { get; set; }

    /// <summary>
    /// 显示属性，包含原因的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}

/// <summary>
/// 命运2举报原因分类清单，包含所有举报原因分类的定义集合
/// </summary>
public class Destiny2ManifestReportReasonCategoryDto : Dictionary<string, Destiny2ManifestReportReasonCategoryDefinitionDto>
{
}