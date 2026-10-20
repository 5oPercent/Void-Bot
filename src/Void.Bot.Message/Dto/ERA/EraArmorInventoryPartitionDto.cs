using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>单个护甲背包分区的计数，不含身上装备。</summary>
public sealed class EraArmorInventoryPartitionDto
{
    /// <summary>唯一计数分区。</summary>
    public EraArmorSlot Slot { get; set; }
    /// <summary>该分区当前背包实例数，不受候选筛选影响。</summary>
    public int Count { get; set; }
    /// <summary>该分区容量。</summary>
    public int Capacity { get; set; }
}
