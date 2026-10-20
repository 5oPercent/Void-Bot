using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>仅后台调度的内部命令，无 HTTP 路由；事务中重新读取服务器时间和存活状态。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class CheckEraWorldTimeoutCommand : ICommand
{
    public Guid WorldInstanceId { get; set; }
}

public sealed class CheckEraWorldTimeoutResponse : IResponse
{
    public EraWorldTimeoutStatus Status { get; set; }
}
