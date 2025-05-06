namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2装备配置常量定义，描述游戏中装备配置系统的全局设置
/// </summary>
public class Destiny2ManifestLoadoutConstantsDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含装备配置系统的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 白色图标路径，用于在亮色背景上显示的装备配置图标
    /// </summary>
    public string WhiteIconImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 黑色图标路径，用于在暗色背景上显示的装备配置图标
    /// </summary>
    public string BlackIconImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 每角色装备配置数量，定义每个角色可以保存的装备配置总数
    /// </summary>
    public int LoadoutCountPerCharacter { get; set; }

    /// <summary>
    /// 装备配置预览过滤排除的插槽类别哈希列表，定义在预览中不显示的插槽类别
    /// </summary>
    public List<uint> LoadoutPreviewFilterOutSocketCategoryHashes { get; set; } = [];
    
    /// <summary>
    /// 装备配置预览过滤排除的插槽类型哈希列表，定义在预览中不显示的插槽类型
    /// </summary>
    public List<uint> LoadoutPreviewFilterOutSocketTypeHashes { get; set; } = [];

    /// <summary>
    /// 装备配置名称哈希列表，包含预设装备配置名称的哈希值
    /// </summary>
    public List<uint> LoadoutNameHashes { get; set; } = [];

    /// <summary>
    /// 装备配置图标哈希列表，包含可用于装备配置的图标哈希值
    /// </summary>
    public List<uint> LoadoutIconHashes { get; set; } = [];

    /// <summary>
    /// 装备配置颜色哈希列表，包含可用于装备配置的颜色哈希值
    /// </summary>
    public List<uint> LoadoutColorHashes { get; set; } = [];
}

/// <summary>
/// 命运2装备配置常量清单，包含装备配置系统全局设置的定义集合
/// </summary>
public class Destiny2ManifestLoadoutConstantsDto : Dictionary<string, Destiny2ManifestLoadoutConstantsDefinitionDto>
{
}