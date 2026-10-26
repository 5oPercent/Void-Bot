namespace Void.Bot.Core.Services.ERA.Equipment;

/// <summary>护甲和武器共用的按权重不放回抽样；不负责实例编号或持久化。</summary>
internal static class EraEquipmentModificationSampler
{
    internal static List<Guid> Sample(IEnumerable<(Guid DefinitionId, int Weight)> pool, int count, int maximum, Func<int, int> next)
    {
        var available = pool.ToList();
        if (count < 1 || count > maximum || count > available.Count || available.Any(row => row.Weight <= 0) ||
            available.Select(row => row.DefinitionId).Distinct().Count() != available.Count)
            throw new ArgumentException("Equipment candidate pool cannot satisfy CandidateCount.");
        long total = available.Sum(row => (long)row.Weight);
        if (total > int.MaxValue) throw new ArgumentException("Equipment pool total weight exceeds its limit.");
        var result = new List<Guid>(count);
        for (var index = 0; index < count; index++)
        {
            var roll = next((int)total);
            if (roll < 0 || roll >= total) throw new ArgumentOutOfRangeException(nameof(next));
            var selected = 0;
            while (roll >= available[selected].Weight) roll -= available[selected++].Weight;
            result.Add(available[selected].DefinitionId);
            total -= available[selected].Weight;
            available.RemoveAt(selected);
        }
        return result;
    }
}
