using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraCharacters")]
public sealed class EraCharactersEndpointContextTemplate : IContextTemplate
{
    public string GetEraCharacterList { get; set; } = "获取本人 ERA 角色列表";
    public string GetEraCharacter { get; set; } = "获取本人指定 ERA 角色";
    public string GetEraCharacterEquipment { get; set; } = "读取本人 ERA 角色背包与已穿装备快照";
    public string GetEraCharacterInventory { get; set; } = "读取本人 ERA 角色背包与槽位候选";
    public string EquipEraEquipment { get; set; } = "装配或原子替换本人 ERA 角色护甲";
    public string UnequipEraEquipment { get; set; } = "将本人 ERA 角色已穿护甲卸下回包";
    public string CreateEraCharacter { get; set; } = "幂等创建本人 ERA 角色";
    public string DeleteEraCharacter { get; set; } = "禁用本人 ERA 角色";
}
