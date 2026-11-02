using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraAbilitySystems")]
public sealed class EraLatticesEndpointContextTemplate : IContextTemplate
{
    public string GetEraSkillProfile { get; set; } = "读取本人谐振技能配装";
    public string SaveEraSkillProfile { get; set; } = "保存本人谐振技能配装";
}
