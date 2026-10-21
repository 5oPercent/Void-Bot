using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>同一次数据库查询读取的背包与已穿状态，两种角色查询接口共用此快照。</summary>
public sealed class EraCharacterEquipmentStateDto
{
    /// <summary>快照所属角色。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>角色是否禁用；禁用角色仍允许本人读取，不能执行新增装备写操作。</summary>
    public bool IsCharacterDisabled { get; set; }
    /// <summary>背包与已穿装备共享的状态版本，初始为 0；不等于角色资料版本。</summary>
    public long EquipmentVersion { get; set; }
    /// <summary>同一快照内读取的世界限制；为空表示当前角色没有仍在继续的世界。</summary>
    public EraEquippedOperationRestrictionDto? EquippedOperationRestriction { get; set; }
    /// <summary>用于本次展示的当前目录版本；不可用时为空，不影响保存属性读取。</summary>
    public long? ManifestVersion { get; set; }
    /// <summary>仅背包接口可指定的目标槽位筛选；限定对应实例分区并检查当前定义兼容关系，定义缺失时按分区降级展示。</summary>
    public EraArmorSlot? FilterSlot { get; set; }
    /// <summary>角色背包装备共享的总容量，当前为 120；不含已穿装备。</summary>
    public int TotalInventoryCapacity { get; set; }
    /// <summary>所有护甲分区的背包实例数，不受筛选影响。</summary>
    public int TotalInventoryCount { get; set; }
    /// <summary>全部六个分类的计数，不受筛选影响；分类不设独立容量。</summary>
    public List<EraArmorInventoryPartitionDto> InventoryPartitions { get; set; } = new List<EraArmorInventoryPartitionDto>();
    /// <summary>当前背包实例或筛选后的候选，按生成时间、实例 ID 稳定排序。</summary>
    public List<EraEquipmentInstanceDto> InventoryItems { get; set; } = new List<EraEquipmentInstanceDto>();
    /// <summary>全部六个已穿槽位；与背包来自同一个资产快照。</summary>
    public List<EraEquippedArmorSlotDto> EquippedSlots { get; set; } = new List<EraEquippedArmorSlotDto>();
}
