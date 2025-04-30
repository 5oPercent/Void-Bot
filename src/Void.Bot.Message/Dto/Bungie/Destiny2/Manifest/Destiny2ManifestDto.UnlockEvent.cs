namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2解锁事件定义，描述游戏中触发特定解锁条件的事件
/// </summary>
public class Destiny2ManifestUnlockEventDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 序列最后更新的解锁值哈希，关联到记录序列上次更新状态的值
    /// </summary>
    public uint SequenceLastUpdatedUnlockValueHash { get; set; }
    
    /// <summary>
    /// 序列解锁值哈希，关联到控制解锁序列进度的值
    /// </summary>
    public uint SequenceUnlockValueHash { get; set; }

    /// <summary>
    /// 新序列奖励站点哈希，关联到提供新解锁序列奖励的位置
    /// </summary>
    public uint NewSequenceRewardSiteHash { get; set; }

    /// <summary>
    /// 解锁条目列表，包含此事件触发的所有解锁项
    /// </summary>
    public List<Destiny2ManifestUnlockEntryDto> UnlockEntries { get; set; } = [];
}

/// <summary>
/// 命运2解锁条目，描述单个解锁项的详细信息和状态
/// </summary>
public class Destiny2ManifestUnlockEntryDto
{
    /// <summary>
    /// 解锁哈希，关联到被解锁的具体内容
    /// </summary>
    public uint UnlockHash { get; set; }

    /// <summary>
    /// 解锁值哈希，关联到追踪解锁进度的计数器
    /// </summary>
    public uint UnlockValueHash { get; set; }

    /// <summary>
    /// 选中值，表示解锁内容被选中时的状态值
    /// </summary>
    public int SelectedValue { get; set; }

    /// <summary>
    /// 清除值，表示解锁内容被清除或重置时的状态值
    /// </summary>
    public int ClearedValue { get; set; }
}

/// <summary>
/// 命运2解锁事件清单，存储所有解锁事件定义的字典集合
/// </summary>
public class Destiny2ManifestUnlockEventDto : Dictionary<string, Destiny2ManifestUnlockEventDefinitionDto>
{
}