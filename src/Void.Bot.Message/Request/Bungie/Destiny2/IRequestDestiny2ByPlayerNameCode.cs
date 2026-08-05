using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

/// <summary>
/// 表示请求可以通过 Bungie 名称和编号指定要查询的 Destiny 2 玩家。
/// </summary>
/// <remarks>
/// 具体处理逻辑位于 Core 项目的
/// <c>Middlewares/Preload/LoadDestiny2PlayerSpecification.cs</c>：当
/// <see cref="DisplayName"/> 或 <see cref="DisplayNameCode"/> 任一不为空时，
/// 中间件优先根据这两个字段加载指定玩家，而不依赖当前账号绑定的 Bungie 数据；
/// 加载完成后还会补充缺失的名称、编号和平台字段。
/// </remarks>
public interface IRequestDestiny2ByPlayerNameCode
{
    string DisplayName { get; set; }
    
    string DisplayNameCode { get; set; }

    BungieMembershipTypeEnum MembershipType { get; set; }
}