using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>技能草稿管理消息，保存不直接发布。</summary>
public sealed class SaveEraDraftSkillCommand : ICommand
{
    /// <summary>路由写入的稳定对象 ID。</summary>
    public string Id { get; set; } = "";
    /// <summary>单项预期版本；创建为零。</summary>
    public long ExpectedVersion { get; set; }
    /// <summary>当前对象完整定义，不是整批配置。</summary>
    public SkillDefinition Definition { get; set; } = new();
}

/// <summary>技能草稿操作结果。</summary>
public sealed class SaveEraDraftSkillResponse : EraResponse<EraSkillDraftItem<SkillDefinition>>
{
}
