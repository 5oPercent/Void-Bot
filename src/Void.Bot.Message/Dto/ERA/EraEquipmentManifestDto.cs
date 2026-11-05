using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>装备目录与当前生成规则；版本不代表角色装备并发版本。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraEquipmentManifestDto
{
    /// <summary>目录契约版本，当前为 4。</summary>
    public int SchemaVersion { get; set; } = 4;
    /// <summary>当前关系表读取及写入并发令牌，不对应历史快照。</summary>
    public long Version { get; set; }
    /// <summary>关系表中的护甲模板、槽位方案与改装配置。</summary>
    public EraArmorCatalogDto Armors { get; set; } = new();
    /// <summary>由后端生成器提供，忽略配置载荷内同名字段。</summary>
    public EraArmorGenerationRulesDto GenerationRules { get; set; } = new EraArmorGenerationRulesDto();
    /// <summary>角色背包装备共享的总容量，不含已穿装备。</summary>
    public int InventoryCapacity { get; set; }
    /// <summary>关系表中的当前武器配置，与目录 Version 对应同一次一致性读取。</summary>
    public EraWeaponCatalogDto Weapons { get; set; } = new EraWeaponCatalogDto();
}
