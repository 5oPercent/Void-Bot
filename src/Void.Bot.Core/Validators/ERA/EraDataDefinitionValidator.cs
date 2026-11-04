using System.Text.Json;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>各类别的单项结构校验；保存时结合只读目录校验完整引用。</summary>
public static class EraDataDefinitionValidator
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
    public static void System(LatticeDefinition value)
    {
        Id(value.Id); AbilityExecutionContract.Key(value.PresentationKey, true); Text(value.Name,256); Text(value.Description,2048);
        Ids(value.SpecialSkills); Ids(value.PrimarySkills); Ids(value.SecondarySkills); Ids(value.TacticalSkills);
    }
    public static void Resonance(ResonanceDefinition value)
    {
        Id(value.Id); Text(value.Name,256); Text(value.Description,2048);
        if (value.Variants == null || value.Variants.Count is < 1 or > 4 || value.Variants.Any(v => v == null) ||
            value.Variants.Select(v => v.Slot).Distinct().Count() != value.Variants.Count) throw new ArgumentException("方案为空或槽位重复。");
        foreach (var variant in value.Variants) AbilityExecutionContract.Variant(variant);
    }
    public static void Definition(EraDataKind kind,object value)
    {
        if (value == null) throw new ArgumentException("定义不能为空。");
        switch (kind)
        {
            case EraDataKind.Lattice: System((LatticeDefinition)value); break;
            case EraDataKind.Resonance: Resonance((ResonanceDefinition)value); break;
            case EraDataKind.Buff: BuffDefinitionValidator.Validate((BuffDefinitionDto)value); break;
            case EraDataKind.Enemy: EnemyDefinitionValidator.Validate((EnemyDefinition)value); break;
            default: throw new ArgumentException("配置类别无效。");
        }
        if (JsonSerializer.SerializeToUtf8Bytes(value,value.GetType()).Length > 262144) throw new ArgumentException("单项定义超过 256 KiB。");
    }
    public static string DefinitionId(object value) => value switch
    {
        LatticeDefinition system => system.Id,
        ResonanceDefinition skill => skill.Id,
        BuffDefinitionDto buff => buff.Id,
        EnemyDefinition enemy => enemy.Id,
        _ => throw new ArgumentException("配置对象类型无效。")
    };
}
