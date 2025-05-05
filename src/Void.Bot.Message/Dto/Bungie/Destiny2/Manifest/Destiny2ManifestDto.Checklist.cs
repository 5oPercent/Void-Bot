using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2清单列表定义，描述游戏中可收集或完成的项目集合
/// </summary>
public class Destiny2ManifestChecklistDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含清单列表的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 查看动作字符串，显示在界面上的交互提示文本
    /// </summary>
    public string ViewActionString { get; set; } = string.Empty;

    /// <summary>
    /// 适用范围，定义清单在个人资料还是角色级别追踪
    /// </summary>
    public Destiny2ScopeEnum Scope { get; set; }

    /// <summary>
    /// 清单项列表，包含此清单中的所有可收集或完成的具体项目
    /// </summary>
    public List<Destiny2ManifestChecklistEntryDefinitionDto> Entries { get; set; } = [];
}

/// <summary>
/// 命运2清单项定义，描述清单中的单个可收集或完成项目
/// </summary>
public class Destiny2ManifestChecklistEntryDefinitionDto
{
    /// <summary>
    /// 哈希值，用于唯一标识此清单项
    /// </summary>
    public uint Hash { get; set; }

    /// <summary>
    /// 显示属性，包含清单项的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 目的地哈希，关联到此清单项所在的目的地
    /// </summary>
    public uint DestinationHash { get; set; }

    /// <summary>
    /// 位置哈希，关联到此清单项的具体位置
    /// </summary>
    public uint LocationHash { get; set; }

    /// <summary>
    /// 区域哈希，关联到此清单项所在的游戏区域
    /// </summary>
    public uint BubbleHash { get; set; }

    /// <summary>
    /// 活动哈希，关联到获取此清单项需完成的活动
    /// </summary>
    public uint ActivityHash { get; set; }

    /// <summary>
    /// 物品哈希，关联到此清单项对应的物品
    /// </summary>
    public uint ItemHash { get; set; }
    
    /// <summary>
    /// 商人哈希，关联到与此清单项相关的商人
    /// </summary>
    public uint VendorHash { get; set; }

    /// <summary>
    /// 商人交互索引，指定与商人进行交互的具体选项
    /// </summary>
    public int VendorInteractionIndex { get; set; }

    /// <summary>
    /// 适用范围，定义此清单项在个人资料还是角色级别追踪
    /// </summary>
    public Destiny2ScopeEnum Scope { get; set; }
}

/// <summary>
/// 命运2清单列表清单，包含所有清单列表的定义集合
/// </summary>
public class Destiny2ManifestChecklistDto : Dictionary<string, Destiny2ManifestChecklistDefinitionDto>
{
}