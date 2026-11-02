using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>固定谐振配置的结构、条件和数值边界。</summary>
public static class AbilityExecutionContract
{
    public static void Key(string? key, bool optional = false)
    {
        if (optional && key == string.Empty) return;
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || key != key.Trim()) throw new ArgumentException("配置 Key 无效。");
    }
    public static void Condition(ResonanceEntityConditionDto? definition)
    {
        int count = 0;
        void Visit(ResonanceEntityConditionDto? value, int depth)
        {
            if (value == null || !Enum.IsDefined(value.Kind) || depth > 8 || ++count > 64 || value.Children == null || value.Children.Count > 16)
                throw new ArgumentException("条件树无效，最多 8 层、64 个节点。");
            bool composite = value.Kind is ResonanceConditionKind.AllOf or ResonanceConditionKind.AnyOf or ResonanceConditionKind.Not;
            bool identity = value.Kind is ResonanceConditionKind.Species or ResonanceConditionKind.Faction;
            if (composite && (value.Children.Count == 0 || value.Kind == ResonanceConditionKind.Not && value.Children.Count != 1)
                || !composite && value.Children.Count != 0 || !identity && value.IdentityId != string.Empty
                || value.Kind == ResonanceConditionKind.Relation && (value.Relation is < 1 or > 4)
                || value.Kind != ResonanceConditionKind.Relation && value.Relation != 0)
                throw new ArgumentException("条件节点的字段或组合无效。");
            if (identity) Key(value.IdentityId);
            foreach (var child in value.Children) Visit(child, depth + 1);
        }
        Visit(definition, 1);
    }
    public static void Variant(ResonanceVariant variant)
    {
        if (variant == null || !Enum.IsDefined(variant.Slot) || !Enum.IsDefined(variant.RecoveryStart)
            || !Enum.IsDefined(variant.CastMode) || !Enum.IsDefined(variant.CarrierKind)
            || variant.MaximumCharges is < 1 or > 100 || !double.IsFinite(variant.CooldownSeconds) || variant.CooldownSeconds is < 0 or > 86400
            || !double.IsFinite(variant.MaximumDistance) || variant.MaximumDistance != -1 && (variant.MaximumDistance is < 0 or > 1000000)
            || !double.IsFinite(variant.ExecutionDelaySeconds) || variant.ExecutionDelaySeconds is < 0 or > 86400)
            throw new ArgumentException("谐振次数、冷却、距离、延迟或枚举无效。");
        Key(variant.ActionKey, true);
        bool persistent = variant.CarrierKind is ResonanceCarrierKind.Construct or ResonanceCarrierKind.Area;
        if (!double.IsFinite(variant.DurationSeconds) || !double.IsFinite(variant.TriggerIntervalSeconds)
            || persistent && (variant.DurationSeconds is < .001 or > 86400 || variant.TriggerIntervalSeconds is < .001 or > 86400)
            || !persistent && (variant.DurationSeconds != 0 || variant.TriggerIntervalSeconds != 0))
            throw new ArgumentException("载体持续时间或触发间隔无效。");
        Condition(variant.Condition);
        if (variant.Effects == null || variant.Effects.Count > 64) throw new ArgumentException("每槽位最多给予 64 项效果。");
        foreach (var effect in variant.Effects)
        {
            if (effect == null) throw new ArgumentException("给予效果不能为空。");
            Key(effect.EffectId);
            if (effect.Condition != null) Condition(effect.Condition);
        }
    }
}
