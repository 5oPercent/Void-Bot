using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取当前玩家已学习的技能 ID。
/// </summary>
public class GetEraLearnedSkillsRequest : IRequest, IRequireEraPlayer
{

}

/// <summary>
/// 读取当前玩家已学习的技能 ID。响应，返回 List<string>。
/// </summary>
public class GetEraLearnedSkillsResponse : EraResponse<List<string>>
{
}
