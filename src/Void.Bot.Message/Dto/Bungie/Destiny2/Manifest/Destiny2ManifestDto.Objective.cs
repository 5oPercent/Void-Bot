using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 目标定义，描述游戏中的任务、挑战或成就目标
/// </summary>
public class Destiny2ManifestObjectiveDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含名称、描述和图标等信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 完成值，目标达成所需的数值
    /// </summary>
    public int CompletionValue { get; set; }
    
    /// <summary>
    /// 作用域，表示目标的权限范围级别
    /// </summary>
    public Destiny2GatingScopeEnum Scope { get; set; }

    /// <summary>
    /// 地点哈希，关联到此目标所在的地点
    /// </summary>
    public uint LocationHash { get; set; }

    /// <summary>
    /// 是否允许负值，目标进度是否可以为负数
    /// </summary>
    public bool AllowNegativeValue { get; set; }

    /// <summary>
    /// 完成后是否允许值变化，目标达成后进度值是否可以继续更新
    /// </summary>
    public bool AllowValueChangeWhenCompleted { get; set; }

    /// <summary>
    /// 是否倒计时，目标是递减而非递增
    /// </summary>
    public bool IsCountingDownward { get; set; }

    /// <summary>
    /// 值的显示样式，定义目标进度的显示方式
    /// </summary>
    public Destiny2DestinyUnlockValueUIStyleEnum ValueStyle { get; set; }

    /// <summary>
    /// 进度描述，解释如何实现目标的文本
    /// </summary>
    public string ProgressDescription { get; set; } = string.Empty;

    /// <summary>
    /// 相关特性，目标关联的特性条目信息
    /// </summary>
    public Destiny2ManifestObjectivePerkEntryDefinitionDto Perks { get; set; } = new();

    /// <summary>
    /// 相关统计，目标影响的统计数据信息
    /// </summary>
    public Destiny2ManifestObjectiveStatEntryDefinitionDto Stats { get; set; } = new();

    /// <summary>
    /// 最小可见阈值，目标在UI中显示所需的最小进度值
    /// </summary>
    public int MinimumVisibilityThreshold { get; set; }

    /// <summary>
    /// 是否允许超额完成，进度是否可以超过完成值
    /// </summary>
    public bool AllowOvercompletion { get; set; }

    /// <summary>
    /// 完成时是否显示值，目标达成后是否仍显示进度值
    /// </summary>
    public bool ShowValueOnComplete { get; set; }

    /// <summary>
    /// 完成时的值样式，目标达成后进度值的显示方式
    /// </summary>
    public Destiny2DestinyUnlockValueUIStyleEnum CompletedValueStyle { get; set; }

    /// <summary>
    /// 进行中的值样式，目标未完成时进度值的显示方式
    /// </summary>
    public Destiny2DestinyUnlockValueUIStyleEnum InProgressValueStyle { get; set; }

    /// <summary>
    /// UI标签，目标在用户界面中的文本标签
    /// </summary>
    public string UiLabel { get; set; } = string.Empty;

    /// <summary>
    /// UI样式，目标在用户界面中的显示样式
    /// </summary>
    public Destiny2ObjectiveUiStyleEnum UiStyle { get; set; }
}

/// <summary>
/// 目标特性条目定义，描述与目标关联的特殊能力或效果
/// </summary>
public class Destiny2ManifestObjectivePerkEntryDefinitionDto
{
    /// <summary>
    /// 特性哈希，关联到特性定义
    /// </summary>
    public uint PerkHash { get; set; }

    /// <summary>
    /// 样式，特性的显示样式
    /// </summary>
    public int Style { get; set; }
}

/// <summary>
/// 目标统计条目定义，描述目标影响的统计属性
/// </summary>
public class Destiny2ManifestObjectiveStatEntryDefinitionDto
{
    /// <summary>
    /// 统计属性，关联到统计属性定义
    /// </summary>
    public uint Stat { get; set; }

    /// <summary>
    /// 样式，统计属性的显示样式
    /// </summary>
    public int Style { get; set; }
}

/// <summary>
/// 目标清单，存储所有目标定义的字典集合
/// </summary>
public class Destiny2ManifestObjectiveDto : Dictionary<string, Destiny2ManifestObjectiveDefinitionDto>
{
}