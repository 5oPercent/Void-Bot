using System.Text.Json;
using Void.Bot.Core.Services.ERA.Configuration.Models;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Configuration;

/// <summary>按各业务表组合当前配置；不保存整包、历史或版本缓存。</summary>
internal static class EraDataCatalog
{
    public static ResonanceConfiguration Compile(EraDataView view)
    {
        var settings = JsonSerializer.Deserialize<EraDataSettings>(view.Workspace.SettingsPayload) ?? throw new InvalidOperationException("配置设置为空。");
        var result = new ResonanceConfiguration
        {
            Enemies = new(),
            CombatRelations = settings.CombatRelations
        };
        foreach (var record in view.Records.OrderBy(r => r.StableId, StringComparer.Ordinal))
        {
            switch (record.Kind)
            {
                case EraDataKind.Lattice:
                    result.Systems.Add(Decode<LatticeDefinition>(record));
                    break;
                case EraDataKind.Resonance:
                    result.Skills.Add(Decode<ResonanceDefinition>(record));
                    break;
                case EraDataKind.Buff:
                    result.Buffs.Add(Decode<BuffDefinitionDto>(record));
                    break;
                case EraDataKind.Enemy:
                    result.Enemies.Add(Decode<EnemyDefinition>(record));
                    break;
                default:
                    throw new InvalidOperationException("配置类别无效。");
            }
        }

        return result;
    }

    public static T Decode<T>(IEraDataDefinition record)
    {
        var value = JsonSerializer.Deserialize<T>(record.Payload);
        if (value is null)
        {
            throw new InvalidOperationException("配置对象为空。");
        }

        if (value is ResonanceDefinition skill && record is EraSkillDefinition stored)
            skill.IsDisabled = stored.IsDisabled;
        if (value is BuffDefinitionDto effect && record is EraEffectDefinition storedEffect)
            effect.IsDisabled = storedEffect.IsDisabled;
        return value;
    }

    public static string Encode<T>(T definition)
    {
        var payload = JsonSerializer.SerializeToNode(definition)!;
        if (definition is ResonanceDefinition or BuffDefinitionDto)
            payload.AsObject().Remove(nameof(ResonanceDefinition.IsDisabled));
        return payload.ToJsonString();
    }

    public static List<EraDataDiagnostic> Validate(ResonanceConfiguration configuration)
    {
        var errors = new List<EraDataDiagnostic>();
        var skills = configuration.Skills.ToDictionary(s => s.Id, StringComparer.Ordinal);
        var buffs = configuration.Buffs.ToDictionary(s => s.Id, StringComparer.Ordinal);
        void CheckIds(IEnumerable<string> ids, string path, Func<string, bool> valid)
        {
            foreach (var id in ids.Where(id => !valid(id)))
            {
                errors.Add(new() { Code = "era.data_invalid_reference", Path = path, Message = "无效引用：" + id });
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

        foreach (var skill in configuration.Skills)
        {
            foreach (var variant in skill.Variants)
            {
                string path = "Skills/" + skill.Id + "/Variants/" + variant.Slot;
                CheckIds(variant.Effects.Select(effect => effect.EffectId), path + "/Effects", buffs.ContainsKey);
                try
                {
                    AbilityExecutionContract.Variant(variant);
                }
                catch (ArgumentException error)
                {
                    errors.Add(new() { Code = "era.data_invalid_carrier", Path = path, Message = error.Message });
                }
            }
        }

        foreach (var enemy in configuration.Enemies ?? new())
        {
            try { EnemyDefinitionValidator.Validate(enemy); EnemyDefinitionValidator.References(enemy, configuration); }
            catch (ArgumentException error) { errors.Add(new() { Code = "era.data_invalid_enemy", Path = "Enemies/" + enemy?.Id, Message = error.Message }); }
        }

        try
        {
            ResonanceConfigurationValidator.Validate(configuration);
        }
        catch (ArgumentException error)
        {
            errors.Add(new() { Code = "era.data_invalid_configuration", Path = "Configuration", Message = error.Message });
        }

        return errors;
    }

}
