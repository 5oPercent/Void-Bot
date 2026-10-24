using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>当前发布的护甲定义；实例读取不依据此定义重新生成六维。</summary>
public sealed class EraArmorDefinitionDto
{
    /// <summary>区分大小写的稳定定义 ID，不是客户端文件路径。</summary>
    public Guid Id { get; set; }
    /// <summary>展示名称。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>由客户端资源映射解析的图标引用，可为空。</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>展示备注，不包含本轮未实现的特殊词条。</summary>
    public string Note { get; set; } = string.Empty;
    /// <summary>允许装配的护甲槽位，必须非空且不重复。</summary>
    public List<EraEquipmentSlot> CompatibleSlots { get; set; } = new List<EraEquipmentSlot>();
}
