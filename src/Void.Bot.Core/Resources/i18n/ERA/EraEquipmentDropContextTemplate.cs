using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "EraEquipmentDrop")]
public sealed class EraEquipmentDropContextTemplate : IContextTemplate
{
    public string InvalidOperation { get; set; } = "掉落操作参数无效";
    public string NotFound { get; set; } = "掉落不存在或当前玩家无领取资格";
    public string WorldUnavailable { get; set; } = "世界未就绪或已经结束";
    public string CharacterUnavailable { get; set; } = "角色不可用或未继续参与该世界";
    public string VersionConflict { get; set; } = "背包、装备或掉落已变化，请刷新后重试";
    public string InventoryFull { get; set; } = "该装备对应的背包分区已满，掉落保持可领取";
    public string DataUnavailable { get; set; } = "掉落、装备或历史回执数据不完整";
    public string ConfigurationUnavailable { get; set; } = "奖励配置或装备目录不可用";
    public string SourceUnavailable { get; set; } = "该奖励来源或节点尚未配置";
    public string DeliveryUnavailable { get; set; } = "该节点未配置为地面发放";
    public string MembersChanged { get; set; } = "世界成员已变化，请使用原请求标识重试";
    public string IdempotencyConflict { get; set; } = "请求标识或业务事件已用于不同参数";
    public string ShareUnavailable { get; set; } = "装备不具备原始分享资格，不能丢出";
    public string ShareExpired { get; set; } = "装备分享期限已结束，不能再次丢出";
}
