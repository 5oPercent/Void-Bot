using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取完整谐振技能配置。
/// </summary>
public class GetEraResonanceConfigurationRequest : IRequest
{

}

/// <summary>
/// 读取完整谐振技能配置。响应，返回 ResonanceConfiguration。
/// </summary>
public class GetEraResonanceConfigurationResponse : EraResponse<ResonanceConfiguration>
{
}
