using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取指定本人角色已学习的谐振技能 ID。
/// </summary>
public class GetEraLearnedResonancesRequest : IRequest, IRequireEraPlayer
{
    /// <summary>角色 ID，由路由写入。</summary>
    public Guid CharacterId { get; set; }
}

/// <summary>
/// 读取指定本人角色已学习的谐振技能 ID。响应，返回 List<string>。
/// </summary>
public class GetEraLearnedResonancesResponse : EraResponse<List<string>>
{
}
