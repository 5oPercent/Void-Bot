using Void.Bot.Message.Request.ERA;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>
/// 保存指定本人角色的晶格体系选择与完整配装记录。
/// </summary>
public class SaveEraResonanceProfileCommand : ICommand, IRequireEraPlayer
{
    /// <summary>角色 ID，由路由写入。</summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 保存前读取的配装版本，用于并发校验。
    /// </summary>
    public long ExpectedVersion { get; set; }

    /// <summary>
    /// 需要保存的完整配装档案。
    /// </summary>
    public CharacterResonanceProfile Profile { get; set; } = new CharacterResonanceProfile();
}

/// <summary>
/// 保存指定本人角色的晶格体系选择与完整配装记录。响应，返回 CharacterResonanceProfile。
/// </summary>
public class SaveEraResonanceProfileResponse : EraResponse<CharacterResonanceProfile>
{
}
