using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraSkills")]
public sealed class EraSkillsEndpointContextTemplate : IContextTemplate
{
    public string GetEraLearnedSkills { get; set; } = "读取本人学习记录";
}
