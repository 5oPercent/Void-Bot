using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Void.Bot.Core.Services.ERA.Skill.Models;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

/// <summary>唯一草稿汇总器；保留池和效果顺序，不读取数据库或实施玩家数据迁移。</summary>
internal static class EraSkillDraftCompiler
{
    public static SkillConfiguration Compile(EraSkillDraftView view)
    {
        var settings = JsonSerializer.Deserialize<EraSkillDraftSettings>(view.Workspace.SettingsPayload) ?? throw new InvalidOperationException("草稿设置为空。");
        var result = new SkillConfiguration
        {
            Version = view.Published.Version,
            CommonTacticalSkills = settings.CommonTacticalSkills,
            DisabledSkills = settings.DisabledSkills
        };
        foreach (var record in view.Records.Where(r => !r.IsDeleted).OrderBy(r => r.StableId, StringComparer.Ordinal))
        {
            switch (record.Kind)
            {
                case EraSkillDraftKind.System:
                    result.Systems.Add(Decode<AbilitySystemDefinition>(record));
                    break;
                case EraSkillDraftKind.Skill:
                    result.Skills.Add(Decode<SkillDefinition>(record));
                    break;
                case EraSkillDraftKind.Buff:
                    result.Buffs.Add(Decode<BuffDefinitionDto>(record));
                    break;
                default:
                    throw new InvalidOperationException("草稿类别无效。");
            }
        }

        return result;
    }

    public static T Decode<T>(EraSkillDraftRecord record)
    {
        var value = JsonSerializer.Deserialize<T>(record.Payload);
        if (value is null)
        {
            throw new InvalidOperationException("草稿对象为空。");
        }

        return value;
    }

    public static string Content(SkillConfiguration configuration)
    {
        var copy = JsonSerializer.Deserialize<SkillConfiguration>(JsonSerializer.Serialize(configuration))!;
        copy.Version = 0;
        copy.Systems = copy.Systems.OrderBy(s => s.Id, StringComparer.Ordinal).ToList();
        copy.Skills = copy.Skills.OrderBy(s => s.Id, StringComparer.Ordinal).ToList();
        copy.Buffs = copy.Buffs.OrderBy(s => s.Id, StringComparer.Ordinal).ToList();
        foreach (var skill in copy.Skills)
        {
            skill.Variants = skill.Variants.OrderBy(v => v.Slot).ToList();
            foreach (var variant in skill.Variants)
            {
                variant.Parameters = variant.Parameters.OrderBy(p => p.Key, StringComparer.Ordinal).ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
            }
        }

        return JsonSerializer.Serialize(copy);
    }

    public static string Hash(SkillConfiguration configuration)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Content(configuration))));
    }

    public static List<EraSkillDraftDiagnostic> Validate(SkillConfiguration configuration)
    {
        var errors = new List<EraSkillDraftDiagnostic>();
        var skills = configuration.Skills.ToDictionary(s => s.Id, StringComparer.Ordinal);
        var buffs = configuration.Buffs.ToDictionary(s => s.Id, StringComparer.Ordinal);
        void CheckIds(IEnumerable<string> ids, string path, Func<string, bool> valid)
        {
            foreach (var id in ids.Where(id => !valid(id)))
            {
                errors.Add(new() { Code = "era.draft_invalid_reference", Path = path, Message = "无效引用：" + id });
            }
        }

        foreach (var system in configuration.Systems)
        {
            var pools = new[]
            {
                system.SpecialSkills,
                system.PrimarySkills,
                system.SecondarySkills,
                system.TacticalSkills
            };
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                CheckIds(pools[i], "Systems/" + system.Id + "/Slots/" + i, id => skills.TryGetValue(id, out var skill) && skill.Variants.Any(v => (int)v.Slot == slot));
            }
        }

        CheckIds(configuration.CommonTacticalSkills, "CommonTacticalSkills", id => skills.TryGetValue(id, out var skill) && skill.Variants.Any(v => v.Slot == SkillSlotKind.Tactical));
        CheckIds(configuration.DisabledSkills, "DisabledSkills", skills.ContainsKey);
        foreach (var skill in configuration.Skills)
        {
            foreach (var variant in skill.Variants)
            {
                string path = "Skills/" + skill.Id + "/Variants/" + variant.Slot;
                CheckIds(variant.BuffIds, path + "/BuffIds", buffs.ContainsKey);
                try
                {
                    SkillCarrierParameterValidator.Validate(variant, true);
                }
                catch (ArgumentException error)
                {
                    errors.Add(new() { Code = "era.draft_invalid_carrier", Path = path + "/Parameters", Message = error.Message });
                }
            }
        }

        try
        {
            SkillConfigurationValidator.Validate(configuration);
        }
        catch (ArgumentException error)
        {
            errors.Add(new() { Code = "era.draft_invalid_configuration", Path = "Configuration", Message = error.Message });
        }

        return errors;
    }

    public static List<EraSkillDraftReference> Changes(SkillConfiguration before, SkillConfiguration after)
    {
        before = JsonSerializer.Deserialize<SkillConfiguration>(Content(before))!;
        after = JsonSerializer.Deserialize<SkillConfiguration>(Content(after))!;
        var result = new List<EraSkillDraftReference>();
        void Compare<T>(string type, IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, string> id)
        {
            var oldMap = oldItems.ToDictionary(id, StringComparer.Ordinal);
            var newMap = newItems.ToDictionary(id, StringComparer.Ordinal);
            foreach (var key in oldMap.Keys.Union(newMap.Keys).Order(StringComparer.Ordinal))
            {
                string? change = !oldMap.ContainsKey(key) ? "Added" : !newMap.ContainsKey(key) ? "Deleted" : JsonSerializer.Serialize(oldMap[key]) != JsonSerializer.Serialize(newMap[key]) ? "Modified" : null;
                if (change != null)
                {
                    result.Add(new() { Type = change, Id = key, Path = type + "/" + key });
                }
            }
        }

        Compare("Systems", before.Systems, after.Systems, s => s.Id);
        Compare("Skills", before.Skills, after.Skills, s => s.Id);
        Compare("Buffs", before.Buffs, after.Buffs, b => b.Id);
        if (!before.CommonTacticalSkills.SequenceEqual(after.CommonTacticalSkills))
        {
            result.Add(new() { Type = "Modified", Id = "CommonTacticalSkills", Path = "Settings/CommonTacticalSkills" });
        }

        if (!before.DisabledSkills.SequenceEqual(after.DisabledSkills))
        {
            result.Add(new() { Type = "Modified", Id = "DisabledSkills", Path = "Settings/DisabledSkills" });
        }

        return result;
    }
}
