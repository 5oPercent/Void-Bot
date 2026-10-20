using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

public sealed class EraRewardNode
{
    public EraRewardSourceType SourceType { get; set; }
    public string SourceDefinitionId { get; set; } = string.Empty;
    public string RewardNodeId { get; set; } = string.Empty;
    public EraRewardDelivery Delivery { get; set; }
    public string PoolId { get; set; } = string.Empty;
    /// <summary>每名领取人每次事件的独立触发概率，万分比；0 必不触发，10000 必触发。</summary>
    public int ChanceBasisPoints { get; set; }
    /// <summary>触发后有放回抽取的次数；每次按池内权重选择一项。</summary>
    public int Rolls { get; set; }
}
