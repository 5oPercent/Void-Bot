using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraCharacterListRequest : IRequest, IRequireEraPlayer
{
}

public class GetEraCharacterListResponse : EraResponse<List<EraCharacterDto>>
{
    /// <summary>当前玩家最多可用角色数量；禁用角色不占用名额。</summary>
    public int CharacterLimit { get; set; } = 1;
}
