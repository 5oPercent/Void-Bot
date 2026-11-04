using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraSkills")]
public sealed class EraResonancesContextTemplate : IContextTemplate
{
    public string VersionConflict { get; set; } ="谐振技能配置或配装版本已变化，请重新读取";
    public string InvalidLoadout { get; set; } ="谐振技能未学习、槽位不匹配或配装重复";
    public string SkillNotFound { get; set; } ="谐振技能不存在";
    public string ConfigUnavailable { get; set; } ="谐振技能配置不可用";
    public string DataUnavailable { get; set; } = "配置尚未初始化或配置迁移未应用";
    public string InvalidData { get; set; } = "配置参数、定义或引用无效";
}
