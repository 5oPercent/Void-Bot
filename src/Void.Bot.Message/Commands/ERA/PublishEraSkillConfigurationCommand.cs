using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>
/// 管理员整批发布技能配置。
/// </summary>
public class PublishEraSkillConfigurationCommand : ICommand
{
    /// <summary>
    /// 发布前读取的配置版本，用于并发校验。
    /// </summary>
    public long ExpectedVersion { get; set; }

    /// <summary>
    /// 需要整批发布的技能、体系、BUFF 与禁用配置。
    /// </summary>
    public SkillConfiguration Configuration { get; set; } = new SkillConfiguration();
}

/// <summary>
/// 管理员整批发布技能配置。响应，返回 SkillConfiguration。
/// </summary>
public class PublishEraSkillConfigurationResponse : EraResponse<SkillConfiguration>
{
}
