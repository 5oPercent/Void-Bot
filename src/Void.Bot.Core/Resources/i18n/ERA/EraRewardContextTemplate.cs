using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraReward")]
public sealed class EraRewardContextTemplate : IContextTemplate
{
    public string InvalidOperation { get; set; } = "奖励事件参数无效";
    public string IdempotencyConflict { get; set; } = "请求标识或奖励事件已用于不同的参数";
    public string ConfigurationUnavailable { get; set; } = "奖励配置或装备目录无效";
    public string ConfigurationVersionConflict { get; set; } = "奖励配置已更新，请按最新版本重新发布";
    public string SourceUnavailable { get; set; } = "奖励来源或节点尚未配置";
    public string DeliveryUnavailable { get; set; } = "该奖励需要地面掉落流程，不能直接发放到背包";
    public string RecipientUnavailable { get; set; } = "奖励领取角色不存在、归属不符或已禁用";
    public string EventNotFound { get; set; } = "奖励事件不存在";
    public string InventoryFull { get; set; } = "奖励超出背包分区容量，整批未发放；整理后可重试";
    public string DataUnavailable { get; set; } = "已保存的奖励计划或回执不完整";
}
