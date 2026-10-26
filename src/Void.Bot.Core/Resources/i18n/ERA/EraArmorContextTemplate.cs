using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraArmor")]
public sealed class EraArmorContextTemplate : IContextTemplate
{
    public string InvalidConfiguration { get; set; } = "护甲配置无效，请检查适用部位、改装槽、属性修正与候选数量";
    public string VersionConflict { get; set; } = "装备目录版本已变化，请刷新配置后重试";
    public string ReferenceConflict { get; set; } = "配置仍被模板或资产引用，不能删除或破坏引用关系";
    public string NotFound { get; set; } = "护甲模板、改装定义或槽位方案不存在";
    public string Unavailable { get; set; } = "护甲配置暂不可用，请稍后重试";
    public string Unauthorized { get; set; } = "需要有效管理身份";
}
