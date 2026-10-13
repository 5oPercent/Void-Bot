using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraSkills")]
public sealed class EraSkillsContextTemplate : IContextTemplate
{
    public string VersionConflict { get; set; } ="技能配置或配装版本已变化，请重新读取";
    public string InvalidLoadout { get; set; } ="技能未学习、槽位不匹配或配装重复";
    public string SkillNotFound { get; set; } ="技能不存在";
    public string ConfigUnavailable { get; set; } ="技能配置不可用";
    public string DraftUnavailable { get; set; } = "草稿尚未初始化或草稿迁移未应用";
    public string InvalidDraft { get; set; } = "草稿参数、定义或引用无效";
    public string ItemDeleted { get; set; } = "对象已标记删除，请先恢复原对象再编辑";
    public string PublishRequestConflict { get; set; } = "发布请求标识已经用于其他请求";
}
