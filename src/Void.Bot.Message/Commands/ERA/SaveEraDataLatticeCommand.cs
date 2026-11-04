using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>谐振技能配置管理消息，保存直接生效。</summary>
public sealed class SaveEraDataLatticeCommand : ICommand
{
    /// <summary>路由写入的稳定对象 ID。</summary>
    public string Id { get; set; } = "";
    /// <summary>单项预期版本；创建为零。</summary>
    public long ExpectedVersion { get; set; }
    /// <summary>当前对象完整定义，不是整批配置。</summary>
    public LatticeDefinition Definition { get; set; } = new();
}

/// <summary>谐振技能配置操作结果。</summary>
public sealed class SaveEraDataLatticeResponse : EraResponse<EraDataItem<LatticeDefinition>>
{
}
