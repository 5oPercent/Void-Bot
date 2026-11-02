using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>
/// 管理员授予指定角色一个谐振技能的学习状态。
/// </summary>
public class GrantEraLearnedResonanceCommand : ICommand
{
    /// <summary>
    /// 目标角色 ID，由管理接口路由写入。
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// 待授予学习状态的稳定谐振技能 ID。
    /// </summary>
    public string SkillId { get; set; } = string.Empty;
}

/// <summary>
/// 管理员授予指定角色一个谐振技能的学习状态。响应，返回 List<string>。
/// </summary>
public class GrantEraLearnedResonanceResponse : EraResponse<List<string>>
{
}
