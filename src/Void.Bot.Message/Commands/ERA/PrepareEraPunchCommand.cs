using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class PrepareEraPunchCommand : ICommand
{
    public Guid TeamId { get; set; }
    public long AuthorityEpoch { get; set; }
    /// <summary>创建或加入时返回的本次成员 ID；重新加入后必须使用新的 ID。</summary>
    public Guid MemberId { get; set; }
    /// <summary>实际游戏 UDP Socket 的局域网 IPv4 候选地址，不包含公网地址。</summary>
    public List<string> LocalAddresses { get; set; } = [];
    public int LocalPort { get; set; }

}

public class PrepareEraPunchResponse : EraResponse<EraPunchTicketDto>
{
}
