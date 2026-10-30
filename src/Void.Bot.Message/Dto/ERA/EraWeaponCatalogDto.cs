using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>同一发布版本的完整武器配置目录；框架、模板、改装及槽位配置整体校验和发布。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponCatalogDto
{
    /// <summary>可复用的武器框架集合，包含驱动、适用补充属性和能力配置。</summary>
    public List<EraWeaponFrameworkDto> Frameworks { get; set; } = new();
    /// <summary>改装槽位类型集合，用于约束槽位可使用的改装定义。</summary>
    public List<EraWeaponSlotTypeDto> SlotTypes { get; set; } = new();
    /// <summary>改装槽位方案集合，定义槽位、候选池及每槽生成数量。</summary>
    public List<EraWeaponSlotConfigurationDto> SlotConfigurations { get; set; } = new();
    /// <summary>武器模板集合，包含框架引用、全部适用属性值及具体表现资源键。</summary>
    public List<EraWeaponTemplateDto> Templates { get; set; } = new List<EraWeaponTemplateDto>();
    /// <summary>公共武器改装定义集合，供候选池和实例固定候选引用。</summary>
    public List<EraWeaponModDefinitionDto> Modifications { get; set; } = new List<EraWeaponModDefinitionDto>();
}
