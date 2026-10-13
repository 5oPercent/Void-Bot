using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraCharacters")]
public sealed class EraCharactersEndpointContextTemplate : IContextTemplate
{
    public string GetEraCharacterList { get; set; } = "获取本人 ERA 角色列表";
    public string GetEraCharacter { get; set; } = "获取本人指定 ERA 角色";
    public string CreateEraCharacter { get; set; } = "幂等创建本人 ERA 角色";
    public string DeleteEraCharacter { get; set; } = "禁用本人 ERA 角色";
}
