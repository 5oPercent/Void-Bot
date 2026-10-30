using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>可被多个候选池和武器实例引用的公共改装定义。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponModDefinitionDto
{
    /// <summary>改装定义唯一标识，默认生成新 Guid；实例候选保留此引用。</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>改装显示名称，必填且最多 128 字符。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>此改装适用的槽位类型标识，必须引用已配置的槽位类型。</summary>
    public Guid SlotTypeId { get; set; }
    /// <summary>改装说明，最多 2048 字符，可为空字符串。</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>改装图标标识，最多 256 字符，可为空字符串。</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>属性修正集合，最多 22 项，可为空；同一属性与运算方式的组合不得重复。</summary>
    public List<EraWeaponModifierDto> Modifiers { get; set; } = new List<EraWeaponModifierDto>();
}
