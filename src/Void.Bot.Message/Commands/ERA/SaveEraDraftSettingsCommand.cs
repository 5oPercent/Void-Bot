using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>技能草稿管理消息，保存不直接发布。</summary>
public sealed class SaveEraDraftSettingsCommand : ICommand
{
    /// <summary>单项预期版本；创建为零。</summary>
    public long ExpectedVersion { get; set; }
    /// <summary>通用战术池与禁用列表。</summary>
    public EraSkillDraftSettings Settings { get; set; } = new();
}

/// <summary>技能草稿操作结果。</summary>
public sealed class SaveEraDraftSettingsResponse : EraResponse<EraSkillDraftSettingsState>
{
}
