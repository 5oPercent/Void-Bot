namespace Void.Bot.Message.Enum.ERA;

/// <summary>装备写操作类别，和玩家范围内 RequestId 一起识别成功请求。</summary>
public enum EraEquipmentOperation
{
    /// <summary>空槽装配或原子替换。</summary>
    Equip = 1,
    /// <summary>卸下到原背包分区。</summary>
    Unequip = 2
}
