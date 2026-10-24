using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment.Models;

/// <summary>已经完成业务校验的位置变更，不更改实例归属、原始属性或首次生成时间。</summary>
public sealed class EraEquipmentPositionChange
{
    public Guid EquipmentId { get; init; }
    public long ExpectedVersion { get; init; }
    public EraEquipmentLocation PreviousLocation { get; init; }
    public EraEquipmentSlot? PreviousEquippedSlot { get; init; }
    public EraEquipmentLocation Location { get; init; }
    public EraEquipmentSlot? EquippedSlot { get; init; }
}
