using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>
/// 读取指定本人角色的体系选择与全部配装记录。
/// </summary>
public class GetEraSkillProfileRequest : IRequest, IRequireEraPlayer
{
    /// <summary>角色 ID，由路由写入。</summary>
    public Guid CharacterId { get; set; }
}

/// <summary>
/// 读取指定本人角色的体系选择与全部配装记录。响应，返回 CharacterSkillProfile。
/// </summary>
public class GetEraSkillProfileResponse : EraResponse<CharacterSkillProfile>
{
}
