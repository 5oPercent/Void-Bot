using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取完整技能配置。
/// </summary>
public class GetEraSkillConfigurationRequest : IRequest
{

}

/// <summary>
/// 读取完整技能配置。响应，返回 SkillConfiguration。
/// </summary>
public class GetEraSkillConfigurationResponse : EraResponse<SkillConfiguration>
{
}
