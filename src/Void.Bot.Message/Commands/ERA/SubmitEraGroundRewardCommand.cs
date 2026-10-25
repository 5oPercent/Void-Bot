using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>当前房主提交怪物／宝箱／世界事件地面奖励；不接收品阶、数量、属性或领取名单。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class SubmitEraGroundRewardCommand : ICommand
{
    /// <summary>由路由覆盖的世界 ID，兼作事件防重作用域。</summary>
    public Guid WorldInstanceId { get; set; }
    /// <summary>世界范围内的重试标识；重试不得更换来源内容或落点。</summary>
    public Guid RequestId { get; set; }
    /// <summary>当前房主的队伍成员身份，迁移后使用新房主身份重新授权。</summary>
    public Guid MemberId { get; set; }
    /// <summary>当前房主代次，只用于权限校验，不参与业务事件身份。</summary>
    public long AuthorityEpoch { get; set; }
    /// <summary>本入口开放怪物、宝箱或世界事件来源；任务完成条件仍由任务业务验证。</summary>
    public EraRewardSourceType SourceType { get; set; }
    /// <summary>查找后端来源节点配置的定义 ID；同事件更改此值返回冲突。</summary>
    public string SourceDefinitionId { get; set; } = string.Empty;
    /// <summary>跨对象同步与房主迁移保持稳定的来源 ID，不使用本机节点或临时连接 ID。</summary>
    public Guid SourceInstanceId { get; set; }
    /// <summary>不可重复触发的来源固定为 1；同一来源再次有效触发时由客户端保存并递增。</summary>
    public int TriggerIndex { get; set; }
    /// <summary>来源配置内的稳定奖励节点 ID。</summary>
    public string RewardNodeId { get; set; } = string.Empty;
    /// <summary>触发时捕获的原始坐标与朝向，响应丢失后使用原值重试。</summary>
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
}

public sealed class SubmitEraGroundRewardResponse : EraResponse<EraGroundRewardReceiptDto>
{
    public bool IsReplay { get; set; }
}
