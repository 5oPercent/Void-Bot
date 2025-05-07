using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2能量类型定义，描述装备和模组使用的能量类型
/// </summary>
public class Destiny2ManifestEnergyTypeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含能量类型的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 透明图标路径，用于显示没有背景的能量类型图标
    /// </summary>
    public string TransparentIconPath { get; set; } = string.Empty;

    /// <summary>
    /// 是否在界面上显示图标
    /// </summary>
    public bool ShowIcon { get; set; }

    /// <summary>
    /// 能量类型枚举值，对应到能量类型系统中的具体类型
    /// </summary>
    public Destiny2EnergyTypeEnum EnumValue { get; set; }

    /// <summary>
    /// 容量状态哈希，关联到表示能量容量的状态值
    /// </summary>
    public uint CapacityStatHash { get; set; }

    /// <summary>
    /// 消耗状态哈希，关联到表示能量消耗的状态值
    /// </summary>
    public uint CostStatHash { get; set; }
}

/// <summary>
/// 命运2能量类型清单，包含所有能量类型的定义集合
/// </summary>
public class Destiny2ManifestEnergyTypeDto : Dictionary<string, Destiny2ManifestEnergyTypeDefinitionDto>
{
}