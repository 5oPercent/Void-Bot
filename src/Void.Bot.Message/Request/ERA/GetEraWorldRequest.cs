using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>本人世界成员读取持久状态；只读查询不刷新 Redis 或修复房主。</summary>
public sealed class GetEraWorldRequest : IRequest, IRequireEraPlayer
{
    public Guid WorldInstanceId { get; set; }
}

public sealed class GetEraWorldResponse : EraResponse<EraWorldDto>
{
}
