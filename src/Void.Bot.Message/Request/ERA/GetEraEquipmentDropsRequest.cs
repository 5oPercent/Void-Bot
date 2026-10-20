using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>只返回登录玩家当前可领取的私人掉落；不接受查询其他玩家的参数。</summary>
public sealed class GetEraEquipmentDropsRequest : IRequest, IRequireEraPlayer
{
    public Guid WorldInstanceId { get; set; }
}

public sealed class GetEraEquipmentDropsResponse : EraResponse<EraEquipmentDropStateDto>
{
}
