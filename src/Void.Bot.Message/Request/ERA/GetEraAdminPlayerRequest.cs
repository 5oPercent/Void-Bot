using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>官方查询指定玩家容量与版本，便于后续执行带版本校验的调整。</summary>
public class GetEraAdminPlayerRequest : IRequest, IRequireEraTargetPlayer
{
    public Guid PlayerId { get; set; }
}

public class GetEraAdminPlayerResponse : EraResponse<EraPlayerDto>
{
}
