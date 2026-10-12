using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>只读校验队伍码，不改变成员关系。</summary>
public class LookupEraTeamRequest : IRequest, IRequireEraPlayer
{
    public string TeamCode { get; set; } = string.Empty;
}

public class LookupEraTeamResponse : EraResponse<bool>
{
}
