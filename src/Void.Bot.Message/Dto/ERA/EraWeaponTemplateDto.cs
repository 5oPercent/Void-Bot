using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>武器模板配置；关联框架并提供全部基础属性及评分端点，不保存某件武器的运行状态。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponTemplateDto
{
    /// <summary>武器模板唯一标识，默认生成新 Guid；区别于装备实例标识。</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>武器显示名称，必填且最多 128 字符。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>武器图标标识，最多 256 字符，可为空字符串。</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>模板说明，最多 2048 字符，可为空字符串。</summary>
    public string Note { get; set; } = string.Empty;
    /// <summary>是否允许生成新武器实例，默认启用；停用不删除已有实例。</summary>
    public bool IsGenerationEnabled { get; set; } = true;
    /// <summary>具体武器表现资源键，必填且最多 128 字符；与框架决定流程的驱动键区分。</summary>
    public string PresentationKey { get; set; } = string.Empty;
    /// <summary>全部适用的基础属性条目，必须恰好包含六项公共属性和框架启用的补充属性；精度、稳定、操控需配置各自效果端点。</summary>
    public List<EraWeaponStatValueDto> BaseStats { get; set; } = new();
    /// <summary>引用的武器框架标识，必填；用于取得表现驱动、能力组合及补充属性集合。</summary>
    public Guid FrameworkId { get; set; }
    /// <summary>武器弹药类型，默认战术；无限类型的储备容量必须为 -1，战术与重型使用有限备弹。</summary>
    public EraWeaponAmmoType AmmoType { get; set; } = EraWeaponAmmoType.Tactical;
    /// <summary>武器补给份额，单位为发／份，范围为 1～100；显式值覆盖框架默认值。创建时省略或为空则带入框架默认值，更新时省略或为空则保留已有值；读取目录时必须已确定，无限类型不参与分配。</summary>
    public int? AmmoSupply { get; set; }
    /// <summary>独立保存的弱点生命伤害系数，默认 1，范围 0～10000；选择框架时带入，允许修改。</summary>
    public double WeakPointHealthDamageMultiplier { get; set; } = 1;
    /// <summary>弱点防护伤害系数，默认 1，范围 0～10000；与生命系数独立，模板不在运行时继承框架值。</summary>
    public double WeakPointProtectionDamageMultiplier { get; set; } = 1;
    /// <summary>允许装备的位置，必须为 Weapon1～Weapon3 中至少一个且不得重复。</summary>
    public List<EraEquipmentSlot> CompatibleSlots { get; set; } = new List<EraEquipmentSlot>();
    /// <summary>可选的改装槽位方案标识；为空时不采用槽位方案生成改装候选。</summary>
    public Guid? SlotConfigurationId { get; set; }
}
