using System.Text.Json;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>单项结构校验允许未完成引用，完整引用在发布时校验。</summary>
public static class SkillDraftDefinitionValidator
{
    public static void Id(string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 128 || id != id.Trim()) throw new ArgumentException("稳定 ID 无效。");
    }
    public static void Ids(List<string>? ids)
    {
        if (ids == null || ids.Count > 10000 || ids.Distinct(StringComparer.Ordinal).Count() != ids.Count) throw new ArgumentException("ID 列表无效或重复。");
        foreach (var id in ids) Id(id);
    }
    private static void Text(string? value,int limit)
    { if (value == null || value.Length > limit) throw new ArgumentException("文字长度无效。"); }
    public static void System(AbilitySystemDefinition value)
    {
        Id(value.Id); Text(value.Name,256); Text(value.Description,2048);
        Ids(value.SpecialSkills); Ids(value.PrimarySkills); Ids(value.SecondarySkills); Ids(value.TacticalSkills);
    }
    public static void Skill(SkillDefinition value)
    {
        Id(value.Id); Text(value.Name,256); Text(value.Description,2048);
        if (value.Variants == null || value.Variants.Count is < 1 or > 4 || value.Variants.Any(v => v == null) ||
            value.Variants.Select(v => v.Slot).Distinct().Count() != value.Variants.Count) throw new ArgumentException("方案为空或槽位重复。");
        foreach (var v in value.Variants)
        {
            if (!Enum.IsDefined(v.Slot) || !Enum.IsDefined(v.RecoveryStart) || !Enum.IsDefined(v.TargetRelation) ||
                v.MaximumCharges is < 1 or > 100 || !double.IsFinite(v.CooldownSeconds) || v.CooldownSeconds < 0 || v.CooldownSeconds > 86400)
                throw new ArgumentException("技能次数、冷却或枚举无效。");
            Id(v.CarrierId); Ids(v.BuffIds);
            if (v.BuffIds.Count > 64 || v.Parameters == null || v.Parameters.Count > 64) throw new ArgumentException("BUFF 或载体参数预算超限。");
            foreach (var parameter in v.Parameters) { Id(parameter.Key); if (!double.IsFinite(parameter.Value)) throw new ArgumentException("载体参数必须为有限数字。"); }
            SkillCarrierParameterValidator.Validate(v,false);
        }
    }
    public static void Definition(EraSkillDraftKind kind,object value)
    {
        if (value == null) throw new ArgumentException("定义不能为空。");
        switch (kind)
        {
            case EraSkillDraftKind.System: System((AbilitySystemDefinition)value); break;
            case EraSkillDraftKind.Skill: Skill((SkillDefinition)value); break;
            case EraSkillDraftKind.Buff: BuffDefinitionValidator.Validate((BuffDefinitionDto)value); break;
            default: throw new ArgumentException("草稿类别无效。");
        }
        if (JsonSerializer.SerializeToUtf8Bytes(value,value.GetType()).Length > 262144) throw new ArgumentException("单项定义超过 256 KiB。");
    }
    public static string DefinitionId(object value) => value switch
    {
        AbilitySystemDefinition system => system.Id,
        SkillDefinition skill => skill.Id,
        BuffDefinitionDto buff => buff.Id,
        _ => throw new ArgumentException("草稿对象类型无效。")
    };
}
