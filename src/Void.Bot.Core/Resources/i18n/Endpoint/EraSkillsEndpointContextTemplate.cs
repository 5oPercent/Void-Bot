using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint","EraSkills")]
public sealed class EraSkillsEndpointContextTemplate : IContextTemplate
{
    public string GetEraSkillConfiguration { get; set; } = "读取游戏已发布技能配置";
    public string GetEraSkillProfile { get; set; } = "读取本人技能配装";
    public string GetEraLearnedSkills { get; set; } = "读取本人学习记录";
    public string SaveEraSkillProfile { get; set; } = "保存本人技能配装";
    public string GrantEraLearnedSkill { get; set; } = "管理员授予玩家学习记录";
}
