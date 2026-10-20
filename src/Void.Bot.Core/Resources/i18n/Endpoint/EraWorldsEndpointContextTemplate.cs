using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraWorlds")]
public sealed class EraWorldsEndpointContextTemplate : IContextTemplate
{
    public string RegisterEraWorld { get; set; } = "登记独立 ERA 世界";
    public string GetEraWorld { get; set; } = "读取本人 ERA 世界状态";
    public string GetCurrentEraWorld { get; set; } = "读取本人当前 ERA 世界";
    public string ReadyEraWorld { get; set; } = "声明世界构建就绪";
    public string HeartbeatEraWorld { get; set; } = "确认本人继续参与的 ERA 世界存活";
    public string RecoverEraWorld { get; set; } = "完成队伍迁移后恢复原世界权限";
    public string JoinEraWorld { get; set; } = "加入队伍的现有世界";
    public string LeaveEraWorld { get; set; } = "报告本人离开世界";
    public string EndEraWorld { get; set; } = "结束世界或释放失败构建";
    public string SetEraWorldEquipmentRestriction { get; set; } = "声明已穿装备操作限制";
}
