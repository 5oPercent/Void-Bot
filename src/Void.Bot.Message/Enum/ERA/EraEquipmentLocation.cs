namespace Void.Bot.Message.Enum.ERA;

/// <summary>装备当前唯一位置；仓库仅预留状态，不提供本轮存取接口。</summary>
public enum EraEquipmentLocation
{
    /// <summary>尚未拾取的地面装备，没有当前资产所有者。</summary>
    Ground = 1,
    /// <summary>角色背包，不占用已穿槽位。</summary>
    Inventory = 2,
    /// <summary>角色当前已穿装备。</summary>
    Equipped = 3,
    /// <summary>玩家共享仓库，不关联角色。</summary>
    Warehouse = 4,
    /// <summary>已销毁，保留身份供历史回执引用。</summary>
    Destroyed = 5
}
