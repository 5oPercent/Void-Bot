namespace Void.Bot.Core.Services.ERA.Equipment;

/// <summary>角色背包由全部装备类别共享容量，已穿装备不占用背包位置。</summary>
public static class EraEquipmentRules
{
    public const int InventoryCapacity = 120;
    public static readonly Void.Bot.Message.Enum.ERA.EraEquipmentSlot[] ArmorSlots =
        System.Enum.GetValues<Void.Bot.Message.Enum.ERA.EraEquipmentSlot>().Where(slot => (int)slot is >= 1 and <= 6).ToArray();
    public static readonly Void.Bot.Message.Enum.ERA.EraEquipmentSlot[] EquippedSlots =
        ArmorSlots.Concat(new[] { Void.Bot.Message.Enum.ERA.EraEquipmentSlot.Weapon1, Void.Bot.Message.Enum.ERA.EraEquipmentSlot.Weapon2,
            Void.Bot.Message.Enum.ERA.EraEquipmentSlot.Weapon3 }).ToArray();
}
