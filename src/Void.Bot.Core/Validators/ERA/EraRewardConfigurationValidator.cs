using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Core.Services.ERA.Equipment;

namespace Void.Bot.Core.Validators.ERA;

public static class EraRewardConfigurationValidator
{
    public static void Validate(EraRewardRules configuration)
    {
        if (configuration.SchemaVersion != 1 || configuration.Nodes == null || configuration.Nodes.Count > 10000 ||
            configuration.Pools == null || configuration.Pools.Count > 1000)
            throw new ArgumentException("Reward configuration schema is invalid.");

        var pools = new Dictionary<string, EraRewardPool>(StringComparer.Ordinal);
        foreach (var pool in configuration.Pools)
        {
            if (pool == null || string.IsNullOrWhiteSpace(pool.Id) || pool.Id.Length > 128 || pool.Id != pool.Id.Trim() ||
                !pools.TryAdd(pool.Id, pool) || pool.Entries == null || pool.Entries.Count is < 1 or > 1000)
                throw new ArgumentException("Reward pool is invalid.");
            foreach (var entry in pool.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.DefinitionId) || entry.DefinitionId.Length > 128 ||
                    entry.DefinitionId != entry.DefinitionId.Trim() || !System.Enum.IsDefined(entry.Grade) ||
                    entry.Count < 1 || entry.Count > EraEquipmentRules.InventoryCapacity || entry.Weight is < 1 or > 1000000)
                    throw new ArgumentException("Reward pool entry is invalid.");
            }
        }

        var nodes = new HashSet<(EraRewardSourceType, string, string)>();
        foreach (var node in configuration.Nodes)
        {
            if (node == null || !System.Enum.IsDefined(node.SourceType) || !System.Enum.IsDefined(node.Delivery) ||
                string.IsNullOrWhiteSpace(node.SourceDefinitionId) || node.SourceDefinitionId.Length > 128 ||
                node.SourceDefinitionId != node.SourceDefinitionId.Trim() || string.IsNullOrWhiteSpace(node.RewardNodeId) ||
                node.RewardNodeId.Length > 128 || node.RewardNodeId != node.RewardNodeId.Trim() ||
                !nodes.Add((node.SourceType, node.SourceDefinitionId, node.RewardNodeId)) ||
                node.PoolId == null || !pools.TryGetValue(node.PoolId, out var pool) ||
                node.ChanceBasisPoints is < 0 or > 10000 || node.Rolls < 1 || node.Rolls > EraEquipmentRules.InventoryCapacity ||
                (long)node.Rolls * pool.Entries.Max(entry => entry.Count) > EraEquipmentRules.InventoryCapacity)
                throw new ArgumentException("Reward source node or delivery is invalid.");
        }
    }
}
