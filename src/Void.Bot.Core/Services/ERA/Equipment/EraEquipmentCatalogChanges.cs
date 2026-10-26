using System.Text.Json;

namespace Void.Bot.Core.Services.ERA.Equipment;

/// <summary>目录发布只写入变化的记录；集合顺序变化最多影响对应记录。</summary>
internal static class EraEquipmentCatalogChanges
{
    internal static HashSet<Guid> ChangedIds<T>(IEnumerable<T> previous, IEnumerable<T> current, Func<T, Guid> id) =>
        ChangedIds(previous, current, id, value => value);

    internal static HashSet<Guid> ChangedIds<T, TValue>(IEnumerable<T> previous, IEnumerable<T> current,
        Func<T, Guid> id, Func<T, TValue> value)
    {
        var before = previous.ToDictionary(id, row => JsonSerializer.Serialize(value(row)));
        var changed = new HashSet<Guid>();
        foreach (var row in current)
        {
            if (!before.Remove(id(row), out var saved) || saved != JsonSerializer.Serialize(value(row)))
                changed.Add(id(row));
        }
        changed.UnionWith(before.Keys);
        return changed;
    }
}

/// <summary>只有可能使已有引用失效的编号才参与资产查询。</summary>
public sealed class EraEquipmentReferenceChanges
{
    public HashSet<Guid> TemplateIds { get; init; } = new();
    public HashSet<Guid> ModificationIds { get; init; } = new();
    public HashSet<Guid> SlotTypeIds { get; init; } = new();
    public HashSet<Guid> SlotIds { get; init; } = new();
    public bool HasChanges => TemplateIds.Count + ModificationIds.Count + SlotTypeIds.Count + SlotIds.Count > 0;
}
