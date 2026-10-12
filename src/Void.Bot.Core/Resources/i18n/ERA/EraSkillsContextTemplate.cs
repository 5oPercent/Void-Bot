using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraSkills")]
public sealed class EraSkillsContextTemplate : IContextTemplate
{
    public string VersionConflict { get; set; } ="技能配置或配装版本已变化，请重新读取";
    public string InvalidLoadout { get; set; } ="技能未学习、槽位不匹配或配装重复";
    public string SkillNotFound { get; set; } ="技能不存在";
    public string ConfigUnavailable { get; set; } ="技能配置不可用";
}
