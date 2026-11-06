using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>通用 BUFF 定义校验；不依赖谐振技能配置、谐振技能池或槽位。</summary>
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
        var life = definition.Lifecycle;
        if (life == null || life.EventsPerStack < 1 || life.StacksPerThreshold < 1 || life.StacksPerDecay < 1
            || life.DecayMode is < 0 or > 2 || !double.IsFinite(life.DecayDelaySeconds) || life.DecayDelaySeconds < 0 || life.DecayDelaySeconds > 86400
            || !double.IsFinite(life.DecayIntervalSeconds) || life.DecayIntervalSeconds <= 0 || life.DecayIntervalSeconds > 86400
            || life.DecayMode == 2 && definition.DurationSeconds <= 0
            || life.CountEvents && (definition.StackRule != 2 || string.IsNullOrWhiteSpace(life.TriggerEventId) || life.TriggerEventId.Length > 128 || life.TriggerEventId != life.TriggerEventId.Trim())
            || definition.Kind == BuffKind.Instant && (life.CountEvents || life.DecayMode != 0))
            throw new ArgumentException("BUFF 生命周期配置无效。");
        ValidateText(definition.Name, 256);
        ValidateText(definition.Description, 2048);
        ValidateText(definition.StackGroup, 128);

        if (!Enum.IsDefined(typeof(BuffKind), definition.Kind) ||
            !IsFiniteInRange(definition.DurationSeconds, 0, 86400) ||
            (definition.IntervalSeconds != -1 && !IsFiniteInRange(definition.IntervalSeconds, 0.001, 86400)) ||
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

            if (effect.Kind >= BuffEffectKind.Stun)
            {
                bool resistance = effect.Kind is BuffEffectKind.StunResistance or BuffEffectKind.RootResistance or BuffEffectKind.SilenceResistance;
                if (definition.Kind != BuffKind.Periodic || effect.Stat != 0 || effect.Operation != 0 ||
                    (resistance ? effect.Amount < 0 || effect.Amount > 1 : effect.Amount != 0))
                    throw new ArgumentException("控制、抗性与免疫必须为持续效果；抗性为 0～1，其余数值、属性与运算必须为 0。");
                if (effect.Kind <= BuffEffectKind.Silence && (definition.Effects.Count != 1 || definition.DurationSeconds <= 0 ||
                    definition.IntervalSeconds != -1 || definition.MaxStacks != 1 || definition.StackRule == 2 ||
                    !definition.RemoveOnDeath || life.CountEvents || life.DecayMode != 0))
                    throw new ArgumentException("控制必须为独立的单项有限时长效果，单层、整体到期、死亡移除，不使用周期结算或事件累计。");
            }
            else if (effect.Kind == BuffEffectKind.Modifier)
            {
                if (definition.Kind != BuffKind.Periodic ||
                    effect.Stat < 0 || effect.Stat > 9 ||
                    effect.Operation < 0 || effect.Operation > 2 ||
                    (effect.Stat <= 2 || effect.Stat >= 7) && effect.Operation != 0 ||
                    effect.Operation == 2 && effect.Amount < 0)
                {
                    throw new ArgumentException("属性修正 BUFF 配置无效。");
                }
            }
            else if (effect.Amount < 0)
            {
                throw new ArgumentException("伤害、治疗和防护恢复的数值不得为负。");
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
