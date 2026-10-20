using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>本人继续成员报告世界存活；迁移时仍允许，不声明房主或修改代次。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class HeartbeatEraWorldCommand : ICommand, IRequireEraPlayer
{
    /// <summary>由世界路由覆盖。</summary>
    public Guid WorldInstanceId { get; set; }
    /// <summary>加入世界时的成员身份，不接受其他玩家身份。</summary>
    public Guid MemberId { get; set; }
}

public sealed class HeartbeatEraWorldResponse : EraResponse<EraWorldHeartbeatDto>
{
}
