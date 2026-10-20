using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>读取本人仍继续的世界；不存在时成功返回空值，不创建世界。</summary>
public sealed class GetCurrentEraWorldRequest : IRequest, IRequireEraPlayer
{
}

public sealed class GetCurrentEraWorldResponse : EraResponse<EraWorldDto>
{
}
