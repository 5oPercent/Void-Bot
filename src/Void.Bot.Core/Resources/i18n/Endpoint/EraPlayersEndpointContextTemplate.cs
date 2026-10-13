using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EraPlayers")]
public sealed class EraPlayersEndpointContextTemplate : IContextTemplate
{
    public string GetEraPlayer { get; set; } = "获取本人 ERA 玩家信息";
    public string InitializeEraPlayer { get; set; } = "幂等初始化本人 ERA 玩家";
}
