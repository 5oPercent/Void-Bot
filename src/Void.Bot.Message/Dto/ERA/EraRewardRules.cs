namespace Void.Bot.Message.Dto.ERA;

/// <summary>独立版本化奖励配置；首发为空，不预设正式物品、品阶概率或来源。</summary>
public sealed class EraRewardRules
{
    public int SchemaVersion { get; set; } = 2;
    public List<EraRewardNode> Nodes { get; set; } = new List<EraRewardNode>();
    public List<EraRewardPool> Pools { get; set; } = new List<EraRewardPool>();
}
