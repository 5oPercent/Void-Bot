using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraManifest")]
public sealed class EraManifestEndpointContextTemplate : IContextTemplate
{
    public string GetEraConfig { get; set; } = "获取 ERA 官方属性规则与默认仓库容量";
    public string GetEraSkillConfiguration { get; set; } = "读取游戏已发布技能配置";
    public string GetEraEquipmentManifest { get; set; } = "读取 ERA 装备目录与护甲生成规则";
}
