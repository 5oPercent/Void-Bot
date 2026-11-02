using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

public static class CombatRelationConfigurationValidator
{
    public static void ValidateConditions(ResonanceConfiguration configuration)
    {
        var species = configuration.CombatRelations?.Species?.Select(i => i.Id).ToHashSet(StringComparer.Ordinal) ?? new();
        var factions = configuration.CombatRelations?.Factions?.Select(i => i.Id).ToHashSet(StringComparer.Ordinal) ?? new();
        void Condition(ResonanceEntityConditionDto? condition)
        {
            if (condition == null) return;
            AbilityExecutionContract.Condition(condition);
            if (condition.Kind == ResonanceConditionKind.Species && !species.Contains(condition.IdentityId)
                || condition.Kind == ResonanceConditionKind.Faction && !factions.Contains(condition.IdentityId))
                throw new ArgumentException("指向条件引用未登记的种族或阵营。");
            foreach (var child in condition.Children) Condition(child);
        }
        foreach (var skill in configuration.Skills)
        foreach (var variant in skill.Variants)
        {
            Condition(variant.Condition);
            foreach (var effect in variant.Effects) Condition(effect.Condition);
        }
    }

    public static void Validate(CombatRelationConfigurationDto? value)
    {
        if (value == null) return; // 旧目录没有关系表；不自动填充任何业务身份。
        var species = Definitions(value.Species);
        var factions = Definitions(value.Factions);
        Rules(value.FactionRules, factions);
        Rules(value.SpeciesRules, species);
    }

    private static HashSet<string> Definitions(List<CombatIdentityDefinitionDto>? entries)
    {
        if (entries == null || entries.Count > 256) throw new ArgumentException("身份目录最多 256 项。");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (entry == null || !ValidId(entry.Id) || !ids.Add(entry.Id)
                || string.IsNullOrWhiteSpace(entry.Name) || entry.Name.Length > 128)
                throw new ArgumentException("身份 Key 重复、格式无效或缺少名称。");
        }
        return ids;
    }

    private static bool ValidId(string? id) => !string.IsNullOrEmpty(id) && id.Length <= 128
        && id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');

    private static void Rules(List<CombatRelationRuleDto>? rules, HashSet<string> ids)
    {
        if (rules == null || rules.Count > 1024) throw new ArgumentException("每类关系表最多 1024 项。");
        var directions = new HashSet<(string, string)>();
        foreach (var rule in rules)
        {
            if (rule == null || !ids.Contains(rule.ObserverId) || !ids.Contains(rule.TargetId)
                || rule.Relation is not (2 or 3 or 4) || !directions.Add((rule.ObserverId, rule.TargetId))
                || rule.Bidirectional && rule.ObserverId != rule.TargetId && !directions.Add((rule.TargetId, rule.ObserverId)))
                throw new ArgumentException("关系表引用未知身份、非法关系或重复方向（含双向规则）。");
        }
    }
}
