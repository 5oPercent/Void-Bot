using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>当前房主明确结束或释放构建失败的世界；不用于地图块卸载。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class EndEraWorldCommand : IEraWorldOperation
{
    /// <summary>路由覆盖的世界实例标识。</summary>
    public Guid WorldInstanceId { get; set; }
    /// <summary>既有队伍成员身份，与当前玩家和角色核对。</summary>
    public Guid MemberId { get; set; }
    /// <summary>当前队伍房主代次，不能用世界版本代替。</summary>
    public long AuthorityEpoch { get; set; }
    /// <summary>客户端最后观察到的世界状态版本。</summary>
    public long ExpectedWorldVersion { get; set; }
    /// <summary>仅构建阶段可声明失败；释放登记，不形成可运行世界。</summary>
    public bool ConstructionFailed { get; set; }
}

public sealed class EndEraWorldResponse : EraResponse<EraWorldDto>
{
}
