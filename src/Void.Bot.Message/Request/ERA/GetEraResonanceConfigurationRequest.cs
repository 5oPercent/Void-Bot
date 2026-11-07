using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取完整谐振技能配置。
/// </summary>
public class GetEraResonanceConfigurationRequest : IRequest
{
    /// <summary>地图模板生成使用完整敌人集合；旧客户端默认不请求该扩展字段。</summary>
    public bool IncludeEnemies { get; set; }
}

/// <summary>
/// 读取完整谐振技能配置。响应，返回 ResonanceConfiguration。
/// </summary>
public class GetEraResonanceConfigurationResponse : EraResponse<ResonanceConfiguration>
{
}
