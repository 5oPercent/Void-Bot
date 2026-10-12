using Void.Bot.Message.Request.ERA;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class PrepareEraPunchCommand : ICommand, IRequireEraPlayer
{
    public Guid TeamId { get; set; }
    public long AuthorityEpoch { get; set; }
    /// <summary>创建或加入时返回的本次成员 ID；重新加入后必须使用新的 ID。</summary>
    public Guid MemberId { get; set; }
    /// <summary>实际游戏 UDP Socket 的有效网卡 IPv4 候选（含 VPN），最多 32 个。</summary>
    public List<string> LocalAddresses { get; set; } = [];
    public int LocalPort { get; set; }

}

public class PrepareEraPunchResponse : EraResponse<EraPunchTicketDto>
{
}
