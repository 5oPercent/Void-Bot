using Mediator.Net.Contracts;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>世界写入均由真实玩家及客户端会话发起，外层工作单元统一提交。</summary>
public interface IEraWorldOperation : ICommand, IRequireEraPlayer
{
    Guid WorldInstanceId { get; set; }
    Guid MemberId { get; set; }
    long AuthorityEpoch { get; set; }
    long ExpectedWorldVersion { get; set; }
}
