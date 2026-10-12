using Void.Bot.Message.Request.ERA;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class LeaveEraTeamCommand : ICommand, IRequireEraPlayer
{
    public Guid TeamId { get; set; }
    /// <summary>创建或加入时返回的本次成员 ID；重新加入后必须使用新的 ID。</summary>
    public Guid MemberId { get; set; }
}

public class LeaveEraTeamResponse : EraResponse<bool>
{
}
