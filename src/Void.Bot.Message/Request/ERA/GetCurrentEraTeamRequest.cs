using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>查询登录玩家当前队伍；没有有效队伍时返回成功且 Data 为 null。</summary>
public class GetCurrentEraTeamRequest : IRequest, IRequireEraPlayer
{
}

public class GetCurrentEraTeamResponse : EraResponse<EraTeamDto>
{
}
