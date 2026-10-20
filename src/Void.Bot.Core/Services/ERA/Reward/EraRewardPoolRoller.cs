using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Services.ERA.Reward;

public interface IEraRewardPoolRoller : IScope
{
    IReadOnlyList<EraRewardPoolEntry> Roll(EraRewardNode node, EraRewardPool pool);
}

/// <summary>来源触发概率与池内权重分开计算；抽取结果由事件计划持久化。</summary>
public sealed class EraRewardPoolRoller : IEraRewardPoolRoller
{
    public IReadOnlyList<EraRewardPoolEntry> Roll(EraRewardNode node, EraRewardPool pool)
    {
        return Roll(node, pool, Random.Shared);
    }

    internal IReadOnlyList<EraRewardPoolEntry> Roll(EraRewardNode node, EraRewardPool pool, Random random)
    {
        var result = new List<EraRewardPoolEntry>();
        if (node.ChanceBasisPoints == 0 || (node.ChanceBasisPoints < 10000 && random.Next(10000) >= node.ChanceBasisPoints)) return result;
        var totalWeight = pool.Entries.Sum(entry => (long)entry.Weight);
        for (var roll = 0; roll < node.Rolls; roll++)
        {
            var ticket = random.NextInt64(totalWeight);
            foreach (var entry in pool.Entries)
            {
                if (ticket < entry.Weight)
                {
                    result.Add(entry);
                    break;
                }
                ticket -= entry.Weight;
            }
        }
        return result;
    }
}
