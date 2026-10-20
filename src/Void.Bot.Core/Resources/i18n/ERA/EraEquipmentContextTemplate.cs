using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraEquipment")]
public sealed class EraEquipmentContextTemplate : IContextTemplate
{
    public string ConfigUnavailable { get; set; } = "装备目录尚未初始化或格式无效";
    public string DataUnavailable { get; set; } = "装备数据不完整或状态无效，请稍后重试";
    public string InvalidSlot { get; set; } = "护甲槽位无效";
    public string InvalidOperation { get; set; } = "装备操作参数或预期版本无效";
    public string CharacterDisabled { get; set; } = "禁用角色不能操作装备";
    public string EquipmentNotFound { get; set; } = "装备不存在或不在当前角色背包及已穿位置";
    public string VersionConflict { get; set; } = "装备或目标槽位已变化，请刷新后重新操作";
    public string ConfigVersionConflict { get; set; } = "装备目录版本已变化，请刷新候选后重新操作";
    public string DefinitionUnavailable { get; set; } = "装备定义暂不可用，不能装配该物品";
    public string IncompatibleSlot { get; set; } = "装备不兼容目标槽位";
    public string InventoryFull { get; set; } = "装备原背包分区已满，无法卸下或替换";
    public string IdempotencyConflict { get; set; } = "请求标识已经用于不同的装备操作参数";
    public string OperationsRestricted { get; set; } = "当前世界限制操作已穿装备；空槽装配及背包物品操作仍允许";
}
