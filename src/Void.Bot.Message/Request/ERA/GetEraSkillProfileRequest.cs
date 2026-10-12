using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取当前玩家的体系选择与全部配装记录。
/// </summary>
public class GetEraSkillProfileRequest : IRequest, IRequireEraPlayer
{

}

/// <summary>
/// 读取当前玩家的体系选择与全部配装记录。响应，返回 PlayerSkillProfile。
/// </summary>
public class GetEraSkillProfileResponse : EraResponse<PlayerSkillProfile>
{
}
