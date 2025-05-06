using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 组队查找器可选项定义
/// </summary>
public class Destiny2ManifestFireteamFinderOptionDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 降序排序优先级
    /// </summary>
    public int DescendingSortPriority { get; set; }

    /// <summary>
    /// 所属组的哈希值
    /// </summary>
    public uint GroupHash { get; set; }

    /// <summary>
    /// 选项代码类型
    /// </summary>
    public Destiny2FireteamFinderOptionTypeEnum CodeOptionType { get; set; }

    /// <summary>
    /// 可用性标志
    /// </summary>
    public Destiny2FireteamFinderOptionAvailabilityEnum Availability { get; set; }
    
    /// <summary>
    /// 可见性标志
    /// </summary>
    public Destiny2FireteamFinderOptionVisibilityEnum Visibility { get; set; }

    /// <summary>
    /// UI显示样式
    /// </summary>
    public string UiDisplayStyle { get; set; } = string.Empty;

    /// <summary>
    /// 创建者设置列表
    /// </summary>
    public List<Destiny2ManifestFireteamFinderOptionCreatorSettingDto> CreatorSettings { get; set; } = [];
    
    /// <summary>
    /// 搜索者设置列表
    /// </summary>
    public List<Destiny2ManifestFireteamFinderOptionSearcherSettingDto> SearcherSettings { get; set; } = [];
    
    /// <summary>
    /// 选项值列表
    /// </summary>
    public List<Destiny2ManifestFireteamFinderOptionValueDto> Values { get; set; } = [];

}

/// <summary>
/// 组队查找器创建者设置
/// </summary>
public class Destiny2ManifestFireteamFinderOptionCreatorSettingDto
{
    /// <summary>
    /// 控件设置
    /// </summary>
    public Destiny2ManifestFireteamFinderOptionSettingsControlDto Control { get; set; } = new();
}

/// <summary>
/// 组队查找器选项设置控件
/// </summary>
public class Destiny2ManifestFireteamFinderOptionSettingsControlDto
{
    /// <summary>
    /// 控件类型
    /// </summary>
    public Destiny2FireteamFinderOptionControlTypeEnum Type { get; set; }

    /// <summary>
    /// 最小选择项目数
    /// </summary>
    public int MinSelectedItems { get; set; }

    /// <summary>
    /// 最大选择项目数
    /// </summary>
    public int MaxSelectedItems { get; set; }
}

/// <summary>
/// 组队查找器搜索者设置
/// </summary>
public class Destiny2ManifestFireteamFinderOptionSearcherSettingDto
{
    /// <summary>
    /// 控件设置
    /// </summary>
    public Destiny2ManifestFireteamFinderOptionSettingsControlDto Control { get; set; } = new();

    /// <summary>
    /// 搜索过滤类型
    /// </summary>
    public Destiny2FireteamFinderOptionSearchFilterTypeEnum SearchFilterType { get; set; }
}

/// <summary>
/// 组队查找器选项值
/// </summary>
public class Destiny2ManifestFireteamFinderOptionValueDto
{
    /// <summary>
    /// 可选的空值显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto OptionalNull { get; set; } = new();

    /// <summary>
    /// 可选的格式化字符串
    /// </summary>
    public string OptionalFormatString { get; set; } = string.Empty;

    /// <summary>
    /// 显示格式类型
    /// </summary>
    public Destiny2FireteamFinderOptionDisplayFormatEnum DisplayFormatType { get; set; }

    /// <summary>
    /// 值类型
    /// </summary>
    public Destiny2FireteamFinderOptionValueTypeEnum Type { get; set; }

    /// <summary>
    /// 值定义列表
    /// </summary>
    public List<Destiny2ManifestFireteamFinderOptionValueEntryDto> ValueDefinitions { get; set; } = [];
}

/// <summary>
/// 组队查找器选项值条目
/// </summary>
public class Destiny2ManifestFireteamFinderOptionValueEntryDto
{
    /// <summary>
    /// 显示属性，包含名称、描述和图标等信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 选项值
    /// </summary>
    public uint Value { get; set; }

    /// <summary>
    /// 标志位
    /// </summary>
    public Destiny2FireteamFinderOptionValueFlagEnum Flag { get; set; }
}

/// <summary>
/// 组队查找器可选项清单
/// </summary>
public class Destiny2ManifestFireteamFinderOptionDto : Dictionary<string, Destiny2ManifestFireteamFinderOptionDefinitionDto>
{
}