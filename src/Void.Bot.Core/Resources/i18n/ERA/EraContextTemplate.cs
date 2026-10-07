using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.ERA;

[Language("ERA", "Era")]
public sealed class EraContextTemplate : IContextTemplate
{
    public string Unauthorized { get; set; } = "请先登录有效账号";
    public string PlayerNotFound { get; set; } = "ERA 玩家尚未初始化或不存在";
    public string CharacterNotFound { get; set; } = "ERA 角色不存在或不属于当前玩家";
    public string ConfigUnavailable { get; set; } = "ERA 配置尚未初始化";
    public string VersionConflict { get; set; } = "数据版本已变化，请重新读取后操作";
    public string IdempotencyConflict { get; set; } = "请求标识已用于不同的操作";
    public string CapacityOutOfRange { get; set; } = "仓库容量不能为负或超出支持范围";
}
