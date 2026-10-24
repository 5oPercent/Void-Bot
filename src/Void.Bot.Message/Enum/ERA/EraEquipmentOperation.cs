namespace Void.Bot.Message.Enum.ERA;

/// <summary>装备写操作类别，和玩家范围内 RequestId 一起识别成功请求。</summary>
public enum EraEquipmentOperation
{
    /// <summary>空槽装配或原子替换。</summary>
    Equip = 1,
    /// <summary>卸下到角色共用背包。</summary>
    Unequip = 2,
    /// <summary>领取世界中的私人或分享地面掉落。</summary>
    Pickup = 3,
    /// <summary>丢出具备分享资格的背包装备。</summary>
    Drop = 4,
    SelectWeaponModification = 5
}
