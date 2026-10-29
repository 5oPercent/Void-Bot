using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>成功装备操作的不可变历史回执；不代表装备当前仍处于此位置。</summary>
public sealed class EraEquipmentOperationReceiptDto
{
    /// <summary>原成功请求的幂等标识。</summary>
    public Guid RequestId { get; set; }
    /// <summary>操作所属角色。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>装配／替换、卸下、拾取、丢出、改装或批量销毁。</summary>
    public EraEquipmentOperation Operation { get; set; }
    /// <summary>操作前的角色装备版本。</summary>
    public long PreviousEquipmentVersion { get; set; }
    /// <summary>操作完成时的资产快照，重试返回原值，不重新加载或恢复资产。</summary>
    public EraCharacterEquipmentStateDto State { get; set; } = new EraCharacterEquipmentStateDto();
    /// <summary>仅拾取回执填写，旧回执保持为空。</summary>
    public EraEquipmentDropPickupDto? Pickup { get; set; }
    /// <summary>仅背包丢出回执填写，旧回执保持为空。</summary>
    public EraEquipmentDropCreatedDto? Dropped { get; set; }
    /// <summary>仅地面销毁回执填写，包含整批成功销毁的掉落。</summary>
    public List<EraEquipmentDropDestroyedDto>? DestroyedDrops { get; set; }
}
