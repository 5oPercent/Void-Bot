using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>完成既有队伍迁移后由新房主恢复原世界，保留成员与限制。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class RecoverEraWorldCommand : IEraWorldOperation
{
    /// <summary>路由覆盖的世界实例标识。</summary>
    public Guid WorldInstanceId { get; set; }
    /// <summary>既有队伍成员身份，与当前玩家和角色核对。</summary>
    public Guid MemberId { get; set; }
    /// <summary>当前队伍房主代次，不能用世界版本代替。</summary>
    public long AuthorityEpoch { get; set; }
    /// <summary>客户端最后观察到的世界状态版本。</summary>
    public long ExpectedWorldVersion { get; set; }
}

public sealed class RecoverEraWorldResponse : EraResponse<EraWorldDto>
{
}
