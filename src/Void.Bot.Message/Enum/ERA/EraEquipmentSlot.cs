namespace Void.Bot.Message.Enum.ERA;

/// <summary>装备穿戴位置协议值；头部等部位与武器位置固定，不表示模板方案内可配置的改装槽。</summary>
public enum EraEquipmentSlot
{
    /// <summary>头部。</summary>
    Head = 1,
    /// <summary>身体。</summary>
    Body = 2,
    /// <summary>护手。</summary>
    Hands = 3,
    /// <summary>背包装备部位，不表示物品背包。</summary>
    Backpack = 4,
    /// <summary>腿部。</summary>
    Legs = 5,
    /// <summary>鞋子。</summary>
    Feet = 6,
    /// <summary>武器一的穿戴位置。</summary>
    Weapon1 = 101,
    /// <summary>武器二的穿戴位置。</summary>
    Weapon2 = 102,
    /// <summary>武器三的穿戴位置。</summary>
    Weapon3 = 103
}
