using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>护甲模板；随机六维由品阶规则生成，槽位方案只用于后续新实例。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraArmorTemplateDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsGenerationEnabled { get; set; } = true;
    /// <summary>允许穿戴的角色装备部位，可多选；不是方案内的改装槽位。</summary>
    public List<EraEquipmentSlot> CompatibleSlots { get; set; } = new();
    public Guid? SlotConfigurationId { get; set; }
}
