using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>只校验定义与引用，不依赖游戏节点、学习记录或后端传输。</summary>
public static class SkillConfigurationValidator
{
    public static void Validate(SkillConfiguration configuration)
    {
        if (configuration == null || configuration.SchemaVersion != 1 || configuration.Version < 0)
        {
            throw new ArgumentException("技能配置格式或版本无效。");
        }

        var systems = Index(configuration.Systems, item => item.Id);
        var skills = Index(configuration.Skills, item => item.Id);
        var buffs = Index(configuration.Buffs, item => item.Id);
        BuffDefinitionValidator.ValidateCollection(configuration.Buffs);

        foreach (var skill in skills.Values)
        {
            SkillDraftDefinitionValidator.Skill(skill);
            foreach (var variant in skill.Variants) References(variant.BuffIds,buffs);
        }
        foreach (var system in systems.Values)
        {
            SkillDraftDefinitionValidator.System(system);
            Pool(system.SpecialSkills, SkillSlotKind.Special, skills);
            Pool(system.PrimarySkills, SkillSlotKind.Primary, skills);
            Pool(system.SecondarySkills, SkillSlotKind.Secondary, skills);
            Pool(system.TacticalSkills, SkillSlotKind.Tactical, skills);
        }

        Pool(configuration.CommonTacticalSkills, SkillSlotKind.Tactical, skills);
        References(configuration.DisabledSkills, skills);
        // 为世界快照中的实体／BUFF 状态保留空间，完整配置不得占满默认 1 MiB RPC 帧。
        if (System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(configuration).Length > 262144)
        {
            throw new ArgumentException("技能配置超过当前快照协议的 256 KiB 配置预算。");
        }
    }

    public static void ValidateProfile(SkillConfiguration configuration, PlayerSkillProfile profile, ISet<string>? learnedSkills = null)
    {
        if (profile == null ||
            profile.Version < 0 ||
            profile.Loadouts == null ||
            profile.Loadouts.Count > 256 ||
            profile.Loadouts.Any(l => l == null) || profile.Loadouts.Select(l => l.SystemId).Distinct(StringComparer.Ordinal).Count() != profile.Loadouts.Count)
        {
            throw new ArgumentException("配装格式无效。");
        }

        var systems = configuration.Systems.ToDictionary(s => s.Id, StringComparer.Ordinal);
        foreach (var loadout in profile.Loadouts)
        {
            if (!systems.TryGetValue(loadout.SystemId, out var system) || loadout.Slots == null || loadout.Slots.Length != 4)
            {
                throw new ArgumentException("体系或配装槽位无效。");
            }

            var selected = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < 4; i++)
            {
                var id = loadout.Slots[i];
                if (id == null)
                {
                    continue;
                }

                if (!selected.Add(id) ||
                    !PoolIds(configuration, system, (SkillSlotKind)i).Contains(id, StringComparer.Ordinal) || learnedSkills != null &&
                    !learnedSkills.Contains(id))
                {
                    throw new ArgumentException("技能不在可用技能池、重复装配或尚未学习。");
                }
            }
        }

        if (profile.SelectedSystemId != null && !profile.Loadouts.Any(l => l.SystemId == profile.SelectedSystemId))
        {
            throw new ArgumentException("当前体系没有配装记录。");
        }
    }

    public static IEnumerable<string> PoolIds(SkillConfiguration configuration, AbilitySystemDefinition system, SkillSlotKind slot)
    {
        switch (slot)
        {
            case SkillSlotKind.Special:
                return system.SpecialSkills;
            case SkillSlotKind.Primary:
                return system.PrimarySkills;
            case SkillSlotKind.Secondary:
                return system.SecondarySkills;
            case SkillSlotKind.Tactical:
                return system.TacticalSkills.Concat(configuration.CommonTacticalSkills).Distinct(StringComparer.Ordinal);
            default:
                throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }

    private static Dictionary<string, T> Index<T>(List<T> items, Func<T, string> key)
    {
        if (items == null || items.Count > 10000)
        {
            throw new ArgumentException("定义集合无效。");
        }

        var result = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item == null)
            {
                throw new ArgumentException("定义不可为空。");
            }

            string id = key(item);
            Id(id);
            if (!result.TryAdd(id, item))
            {
                throw new ArgumentException("定义 ID 重复。");
            }
        }

        return result;
    }

    private static void References<T>(List<string> ids, Dictionary<string, T> definitions)
    {
        if (ids == null ||
            ids.Count > 10000 ||
            ids.Any(id => id == null ||
            !definitions.ContainsKey(id)) || ids.Distinct(StringComparer.Ordinal).Count() != ids.Count)
        {
            throw new ArgumentException("配置引用不存在或重复。");
        }
    }

    private static void Pool(List<string> ids, SkillSlotKind slot, Dictionary<string, SkillDefinition> skills)
    {
        References(ids, skills);
        if (ids.Any(id => !skills[id].Variants.Any(v => v.Slot == slot)))
        {
            throw new ArgumentException("技能池引用不支持该槽位的方案。");
        }
    }

    private static bool ValidId(string? id)
    {
        return !string.IsNullOrWhiteSpace(id) && id.Length <= 128 && id == id.Trim();
    }

    private static void Id(string id)
    {
        if (!ValidId(id))
        {
            throw new ArgumentException("稳定 ID 无效。");
        }
    }


}
