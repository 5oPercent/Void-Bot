namespace Void.Bot.Message.Request.Bungie.Destiny2;

/// <summary>
/// 标记请求在执行前需要加载当前账号绑定的 Destiny 2 玩家数据。
/// </summary>
/// <remarks>
/// 具体处理逻辑位于 Core 项目的
/// <c>Middlewares/Preload/LoadDestiny2PlayerSpecification.cs</c>：中间件通过当前用户的
/// Bungie 第三方绑定加载玩家数据；当前用户不存在或未绑定 Bungie 时会抛出异常。
/// 此接口不包含成员，也不依赖其他玩家查询接口。
/// </remarks>
public interface ILoadDestiny2Player
{
}