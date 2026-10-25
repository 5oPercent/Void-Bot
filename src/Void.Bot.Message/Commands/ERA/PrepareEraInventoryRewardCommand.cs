using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>可信后端业务接受奖励事件。必须单独提交成功后，再调用发放命令；无 HTTP 路由。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class PrepareEraInventoryRewardCommand : ICommand
{
    /// <summary>由接入业务分配的稳定作用域；当前不是世界奖励入口。</summary>
    public Guid ScopeId { get; set; }
    public Guid RequestId { get; set; }
    public EraRewardSourceType SourceType { get; set; }
    public Guid SourceInstanceId { get; set; }
    public string SourceDefinitionId { get; set; } = string.Empty;
    public int TriggerIndex { get; set; }
    public string RewardNodeId { get; set; } = string.Empty;
    /// <summary>不为空时直接发放条目，跳过来源节点与奖励池；仍以业务来源身份防重。</summary>
    public List<EraEquipmentRewardEntry>? Entries { get; set; }
    /// <summary>可信业务确认过的领取名单；客户端不能指定奖励内容或调用本命令。</summary>
    public List<EraRewardRecipient> Recipients { get; set; } = new List<EraRewardRecipient>();
}

public sealed class PrepareEraInventoryRewardResponse : EraResponse<Guid>
{
    public bool IsReplay { get; set; }
}
