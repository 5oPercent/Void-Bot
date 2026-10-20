using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraEquipment")]
public sealed class EraEquipmentContextTemplate : IContextTemplate
{
    public string ConfigUnavailable { get; set; } = "装备目录尚未初始化或格式无效";
    public string DataUnavailable { get; set; } = "装备数据不完整或状态无效，请稍后重试";
    public string InvalidSlot { get; set; } = "护甲槽位无效";
}
