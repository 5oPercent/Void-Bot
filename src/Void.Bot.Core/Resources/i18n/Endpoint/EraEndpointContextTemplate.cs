using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Era")]
public sealed class EraEndpointContextTemplate : IContextTemplate
{
    public string GetEraAdminPlayer { get; set; } = "官方查询指定 ERA 玩家的容量与版本";
    public string GetEraConfig { get; set; } = "获取 ERA 官方属性规则与默认仓库容量";
    public string GetEraPlayer { get; set; } = "获取本人 ERA 玩家信息";
    public string InitializeEraPlayer { get; set; } = "幂等初始化本人 ERA 玩家";
    public string GetEraCharacterList { get; set; } = "获取本人 ERA 角色列表";
    public string GetEraCharacter { get; set; } = "获取本人指定 ERA 角色";
    public string DeleteEraCharacter { get; set; } = "禁用本人 ERA 角色";
    public string CreateEraCharacter { get; set; } = "幂等创建本人 ERA 角色";
    public string UpdateEraConfig { get; set; } = "发布 ERA 官方配置";
    public string SetEraWarehouseCapacity { get; set; } = "设置指定玩家的仓库容量";
    public string AdjustEraWarehouseCapacity { get; set; } = "增减指定玩家的仓库容量";
}
