namespace Void.Bot.Message.Dto.ERA;

/// <summary>装备目录与当前生成规则；版本不代表角色装备并发版本。</summary>
public sealed class EraEquipmentManifestDto
{
    /// <summary>目录契约版本，当前为 1。</summary>
    public int SchemaVersion { get; set; } = 1;
    /// <summary>由配置存储行确定的整份目录发布版本。</summary>
    public long Version { get; set; }
    /// <summary>当前护甲定义，可为空；缺失定义不使历史资产消失。</summary>
    public List<EraArmorDefinitionDto> Definitions { get; set; } = new List<EraArmorDefinitionDto>();
    /// <summary>由后端生成器提供，忽略配置载荷内同名字段。</summary>
    public EraArmorGenerationRulesDto GenerationRules { get; set; } = new EraArmorGenerationRulesDto();
    /// <summary>角色背包装备共享的总容量，不含已穿装备。</summary>
    public int InventoryCapacity { get; set; }
    /// <summary>关系表中的当前武器配置，与目录 Version 属于同一发布。</summary>
    public EraWeaponCatalogDto Weapons { get; set; } = new EraWeaponCatalogDto();
}
