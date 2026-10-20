using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>当前房主登记独立世界；同队同请求只分配一次身份，失败构建也不能复用为新局。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class RegisterEraWorldCommand : ICommand, IRequireEraPlayer
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public Guid RequestId { get; set; }
    public long AuthorityEpoch { get; set; }
    /// <summary>用于恢复的世界定义标识，不作为战斗结果或奖励配置的可信证明。</summary>
    public string WorldDefinitionId { get; set; } = string.Empty;
}

public sealed class RegisterEraWorldResponse : EraResponse<EraWorldDto>
{
}
