using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>通用 BUFF 定义校验；不依赖技能配置、技能池或槽位。</summary>
public static class BuffDefinitionValidator
{
    public static void Validate(BuffDefinitionDto definition)
    {
        ValidateCollection(new List<BuffDefinitionDto>() { definition });
    }

    public static void ValidateCollection(IReadOnlyCollection<BuffDefinitionDto> definitions)
    {
        if (definitions == null || definitions.Count > 10000)
        {
            throw new ArgumentException("BUFF 定义集合无效。");
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id) ||
                definition.Id.Length > 128 || definition.Id != definition.Id.Trim() || !ids.Add(definition.Id))
            {
                throw new ArgumentException("BUFF ID 无效或重复。");
            }

            ValidateDefinition(definition);
        }

        foreach (var group in definitions.GroupBy(GetGroupKey))
        {
            var first = group.First();
            if (group.Any(definition => definition.StackRule != first.StackRule ||
                    definition.MaxStacks != first.MaxStacks || definition.Kind != first.Kind) ||
                group.Count() > 1 && (first.StackRule == 1 || first.StackRule == 2))
            {
                throw new ArgumentException("同组 BUFF 叠加规则不一致，刷新／叠层必须使用同一 BUFF 定义。");
            }
        }
    }

    private static (string Group, int SourceScope) GetGroupKey(BuffDefinitionDto definition)
    {
        var group = definition.StackGroup;
        if (string.IsNullOrEmpty(group))
        {
            group = definition.Id;
        }

        return (group, definition.SourceScope);
    }

    private static void ValidateDefinition(BuffDefinitionDto definition)
    {
        ValidateText(definition.Name, 256);
        ValidateText(definition.Description, 2048);
        ValidateText(definition.StackGroup, 128);

        if (!Enum.IsDefined(typeof(BuffKind), definition.Kind) ||
            !IsFiniteInRange(definition.DurationSeconds, 0, 86400) ||
            !IsFiniteInRange(definition.IntervalSeconds, 0.001, 86400) ||
            definition.StackRule < 0 || definition.StackRule > 3 ||
            definition.SourceScope < 0 || definition.SourceScope > 2 ||
            definition.MaxStacks < 1 || definition.MaxStacks > 10000 ||
            definition.Effects == null || definition.Effects.Count == 0 || definition.Effects.Count > 32)
        {
            throw new ArgumentException("BUFF 时间或叠加配置无效。");
        }

        foreach (var effect in definition.Effects)
        {
            if (effect == null || !Enum.IsDefined(typeof(BuffEffectKind), effect.Kind) ||
                !double.IsFinite(effect.Amount) || Math.Abs(effect.Amount) > float.MaxValue)
            {
                throw new ArgumentException("BUFF 效果无效。");
            }

            if (effect.Kind == BuffEffectKind.Modifier)
            {
                if (definition.Kind != BuffKind.Persistent ||
                    effect.Stat < 0 || effect.Stat > 6 ||
                    effect.Operation < 0 || effect.Operation > 2 ||
                    effect.Stat <= 2 && effect.Operation != 0 ||
                    effect.Operation == 2 && effect.Amount < 0)
                {
                    throw new ArgumentException("属性修正 BUFF 配置无效。");
                }
            }
            else if (effect.Amount < 0 || definition.Kind == BuffKind.Persistent)
            {
                throw new ArgumentException("治疗／伤害应使用即时或周期 BUFF。");
            }
        }
    }

    private static bool IsFiniteInRange(double value, double minimum, double maximum)
    {
        return double.IsFinite(value) && value >= minimum && value <= maximum;
    }

    private static void ValidateText(string text, int maximumLength)
    {
        if (text == null || text.Length > maximumLength)
        {
            throw new ArgumentException("BUFF 文字长度无效。");
        }
    }
}
