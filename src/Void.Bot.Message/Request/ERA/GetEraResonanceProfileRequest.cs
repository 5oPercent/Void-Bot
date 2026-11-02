using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取指定本人角色的晶格体系选择与全部配装记录。
/// </summary>
public class GetEraResonanceProfileRequest : IRequest, IRequireEraPlayer
{
    /// <summary>角色 ID，由路由写入。</summary>
    public Guid CharacterId { get; set; }
}

/// <summary>
/// 读取指定本人角色的晶格体系选择与全部配装记录。响应，返回 CharacterResonanceProfile。
/// </summary>
public class GetEraResonanceProfileResponse : EraResponse<CharacterResonanceProfile>
{
}
