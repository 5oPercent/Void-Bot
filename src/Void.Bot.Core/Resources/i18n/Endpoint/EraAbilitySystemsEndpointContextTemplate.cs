using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraAbilitySystems")]
public sealed class EraAbilitySystemsEndpointContextTemplate : IContextTemplate
{
    public string GetEraSkillProfile { get; set; } = "读取本人技能配装";
    public string SaveEraSkillProfile { get; set; } = "保存本人技能配装";
}
