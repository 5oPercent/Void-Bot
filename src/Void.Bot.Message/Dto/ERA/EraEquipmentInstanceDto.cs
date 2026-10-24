using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>后端已保存的装备实例与适用的当前展示定义。</summary>
public sealed class EraEquipmentInstanceDto
{
    /// <summary>装备唯一身份，转移、拾取和丢出不改变。</summary>
    public Guid Id { get; set; }
    /// <summary>实例引用的稳定定义 ID。</summary>
    public Guid DefinitionId { get; set; }
    /// <summary>首次生成时采用的目录版本；不是展示定义的当前版本。</summary>
    public long DefinitionVersion { get; set; }
    /// <summary>当前目录中的展示定义；缺失或目录不可用时为空，客户端按定义 ID 降级显示。</summary>
    public EraArmorDefinitionDto? Definition { get; set; }
    /// <summary>装备类型，决定护甲或武器载荷。</summary>
    public EraEquipmentType EquipmentType { get; set; }
    /// <summary>本件保存的品阶。</summary>
    public EraEquipmentGrade Grade { get; set; }
    /// <summary>当前资产所属 ERA 玩家 ID。</summary>
    public Guid OwnerPlayerId { get; set; }
    /// <summary>当前所属角色。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>本查询只返回背包或已穿位置。</summary>
    public EraEquipmentLocation Location { get; set; }
    /// <summary>当前已穿槽位；背包实例为空。</summary>
    public EraEquipmentSlot? EquippedSlot { get; set; }
    /// <summary>实例写操作的并发版本。</summary>
    public long Version { get; set; }
    /// <summary>首次生成时间，不随位置变化刷新。</summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>保存结果采用的生成规则版本，读取时不要求等于当前版本。</summary>
    public int GenerationRuleVersion { get; set; }
    /// <summary>已保存的六维原值，不重新生成或按当前预算重新校验。</summary>
    public EraArmorAttributesDto ArmorAttributes { get; set; } = new EraArmorAttributesDto();
    public EraWeaponInstanceDto? Weapon { get; set; }
    public EraWeaponTemplateDto? WeaponTemplate { get; set; }
}
