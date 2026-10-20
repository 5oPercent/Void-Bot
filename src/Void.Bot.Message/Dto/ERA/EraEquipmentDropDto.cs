using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>仅向有资格的玩家返回的当前可拾取私人装备。</summary>
public sealed class EraEquipmentDropDto
{
    public Guid DropId { get; set; }
    public long DropVersion { get; set; }
    public Guid EquipmentId { get; set; }
    public long EquipmentVersion { get; set; }
    public string DefinitionId { get; set; } = string.Empty;
    public long DefinitionVersion { get; set; }
    public EraArmorDefinitionDto? Definition { get; set; }
    public EraArmorGrade Grade { get; set; }
    public EraArmorSlot InventorySlot { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int GenerationRuleVersion { get; set; }
    public EraArmorAttributesDto ArmorAttributes { get; set; } = new EraArmorAttributesDto();
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
}
