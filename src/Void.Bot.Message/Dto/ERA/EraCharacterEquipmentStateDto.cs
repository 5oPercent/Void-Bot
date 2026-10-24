using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>同一资产版本的背包与已穿状态，两种角色查询接口共用此快照。</summary>
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
    /// <summary>仅背包接口可指定的目标穿戴位置筛选。</summary>
    public EraEquipmentSlot? FilterSlot { get; set; }
    /// <summary>仅背包接口可指定的装备类型筛选，不影响共享容量与总数。</summary>
    public EraEquipmentType? FilterEquipmentType { get; set; }
    /// <summary>角色背包装备共享的总容量，当前为 120；不含已穿装备。</summary>
    public int TotalInventoryCapacity { get; set; }
    /// <summary>同一背包内全部武器与护甲的实例数，不受筛选影响。</summary>
    public int TotalInventoryCount { get; set; }
    /// <summary>当前背包实例或筛选后的候选，按生成时间、实例 ID 稳定排序。</summary>
    public List<EraEquipmentInstanceDto> InventoryItems { get; set; } = new List<EraEquipmentInstanceDto>();
    /// <summary>六个护甲与三个武器位置；与背包来自同一个资产快照。</summary>
    public List<EraEquippedArmorSlotDto> EquippedSlots { get; set; } = new List<EraEquippedArmorSlotDto>();
}
