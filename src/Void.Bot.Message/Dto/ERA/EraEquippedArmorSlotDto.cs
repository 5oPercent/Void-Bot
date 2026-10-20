using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>一个护甲槽位与其当前已穿实例；空槽仍返回。</summary>
public sealed class EraEquippedArmorSlotDto
{
    /// <summary>已穿槽位。</summary>
    public EraArmorSlot Slot { get; set; }
    /// <summary>当前已穿实例，空槽为 null。</summary>
    public EraEquipmentInstanceDto? Equipment { get; set; }
}
