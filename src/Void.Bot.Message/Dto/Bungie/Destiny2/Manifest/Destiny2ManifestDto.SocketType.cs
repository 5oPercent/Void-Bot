using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 插槽类型定义，描述游戏中装备插槽的具体类型及其行为特性
/// </summary>
public class Destiny2ManifestSocketTypeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含插槽类型的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 插入插件动作定义，描述插入插件时的效果和动作
    /// </summary>
    public Destiny2ManifestInsertPlugActionDefinitionDto InsertAction { get; set; } = new();

    /// <summary>
    /// 插件白名单，定义哪些插件可以被插入此类型的插槽
    /// </summary>
    public Destiny2ManifestPlugWhitelistEntryDefinitionDto PlugWhitelist { get; set; } = new();

    /// <summary>
    /// 插槽类别哈希，关联到此插槽类型所属的类别
    /// </summary>
    public uint SocketCategoryHash { get; set; }

    /// <summary>
    /// 可见性，定义插槽在界面中的显示状态
    /// </summary>
    public Destiny2SocketVisibilityEnum Visibility { get; set; }

    /// <summary>
    /// 总是随机化插槽，指示该插槽是否始终随机分配插件
    /// </summary>
    public bool AlwaysRandomizeSockets { get; set; }

    /// <summary>
    /// 是否启用预览，决定玩家是否可以预览插件效果
    /// </summary>
    public bool IsPreviewEnabled { get; set; }

    /// <summary>
    /// 隐藏重复的可重用插件，避免在界面中显示相同的插件选项
    /// </summary>
    public bool HideDuplicateReusablePlugs { get; set; }

    /// <summary>
    /// 覆盖UI外观，指示该插槽是否有自定义的界面显示方式
    /// </summary>
    public bool OverridesUiAppearance { get; set; }

    /// <summary>
    /// 初始化时避免重复，在初次生成时避免分配重复的插件
    /// </summary>
    public bool AvoidDuplicatesOnInitialization { get; set; }
    
    /// <summary>
    /// 货币缩放器，调整插入不同品质插件所需材料的数量
    /// </summary>
    public List<Destiny2ManifestSocketTypeScalarMaterialRequirementEntryDto> CurrencyScalars { get; set; } = [];
}

/// <summary>
/// 插入插件动作定义，描述插入插件时的行为和效果
/// </summary>
public class Destiny2ManifestInsertPlugActionDefinitionDto
{
    /// <summary>
    /// 动作执行时间（秒），插入插件需要的时间
    /// </summary>
    public int ActionExecuteSeconds { get; set; }

    /// <summary>
    /// 动作类型，定义插入插件时执行的操作类型
    /// </summary>
    public Destiny2SocketTypeActionTypeEnum ActionType { get; set; }
}

/// <summary>
/// 插件白名单条目定义，限制可插入插槽的插件类型
/// </summary>
public class Destiny2ManifestPlugWhitelistEntryDefinitionDto
{
    /// <summary>
    /// 类别哈希，关联到插件类别的定义
    /// </summary>
    public uint CategoryHash { get; set; }

    /// <summary>
    /// 类别标识符，插件类别的字符串标识
    /// </summary>
    public string CategoryIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// 可能的重置插件哈希列表，用于插槽重置时可选择的插件
    /// </summary>
    public List<uint> ReinitializationPossiblePlugHashes { get; set; } = [];
}

/// <summary>
/// 插槽类型材料需求缩放条目，调整插入插件所需的材料数量
/// </summary>
public class Destiny2ManifestSocketTypeScalarMaterialRequirementEntryDto
{
    /// <summary>
    /// 当前物品哈希，关联到需要缩放材料需求的物品定义
    /// </summary>
    public uint CurrentItemHash { get; set; }

    /// <summary>
    /// 缩放值，用于调整材料需求的数量倍率
    /// </summary>
    public int ScalarValue { get; set; }
}

/// <summary>
/// 插槽类型清单，存储所有插槽类型定义的字典集合
/// </summary>
public class Destiny2ManifestSocketTypeDto : Dictionary<string, Destiny2ManifestSocketTypeDefinitionDto>
{
}