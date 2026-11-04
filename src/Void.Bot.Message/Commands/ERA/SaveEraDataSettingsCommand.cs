using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>谐振技能配置管理消息，保存直接生效。</summary>
public sealed class SaveEraDataSettingsCommand : ICommand
{
    /// <summary>单项预期版本；创建为零。</summary>
    public long ExpectedVersion { get; set; }
    /// <summary>种族、阵营与默认关系。</summary>
    public EraDataSettings Settings { get; set; } = new();
}

/// <summary>谐振技能配置操作结果。</summary>
public sealed class SaveEraDataSettingsResponse : EraResponse<EraDataSettingsState>
{
}
