using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

public static class EnemyDefinitionValidator
{
    private static void Number(double value, double minimum, double maximum)
    {
        if (!double.IsFinite(value) || value < minimum || value > maximum) throw new ArgumentException("敌人数值超出范围。");
    }

    public static void Validate(EnemyDefinition value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.Name) || value.Name.Length > 256 || value.Description == null || value.Description.Length > 2048)
            throw new ArgumentException("敌人名称或说明无效。");
        ResonanceDraftDefinitionValidator.Id(value.Id);
        AbilityExecutionContract.Key(value.PresentationKey, true);
        ResonanceDraftDefinitionValidator.Id(value.SpeciesId);
        ResonanceDraftDefinitionValidator.Id(value.FactionId);
        Number(value.MaximumHealth, 1, 1000000000);
        Number(value.MaximumProtection, 0, 1000000000);
        Number(value.WalkSpeed, 0, 1000); Number(value.RunSpeed, 0, 1000);
        Number(value.RecoveryDelaySeconds, 0, 86400); Number(value.HealthRecoveryPerSecond, 0, 1000000000); Number(value.ProtectionRecoveryPerSecond, 0, 1000000000);
        var ai = value.Ai;
        if (ai == null || !Enum.IsDefined(ai.Behavior) || !Enum.IsDefined(ai.Aggression)) throw new ArgumentException("敌人 AI 行为无效。");
        Number(ai.PerceptionDistance, 0, 1000000); Number(ai.FieldOfView, 0, 360);
        Number(ai.PerceptionIntervalSeconds, 0.001, 86400); Number(ai.TargetMemorySeconds, 0, 86400);
        Number(ai.MaximumChaseSeconds, 0.001, 86400); Number(ai.StopDistance, 0, 1000000);
        Number(ai.StuckTimeoutSeconds, 0.001, 86400);
        Number(ai.IdleMinimumSeconds, 0, 86400); Number(ai.IdleMaximumSeconds, ai.IdleMinimumSeconds, 86400);
        var attack = value.NormalAttack;
        if (attack == null) throw new ArgumentException("缺少普通攻击配置。");
        Number(attack.Damage, 0, 1000000000); Number(attack.Distance, 0, 1000000);
        Number(attack.WindupSeconds, 0, 86400); Number(attack.BackswingSeconds, 0, 86400); Number(attack.CooldownSeconds, 0, 86400);
        if (value.Skills == null || value.Skills.Count > 256 || value.Skills.Any(s => s == null) || value.Skills.Select(s => s.SkillId).Distinct(StringComparer.Ordinal).Count() != value.Skills.Count)
            throw new ArgumentException("敌人技能列表无效或重复。");
        foreach (var skill in value.Skills)
        {
            ResonanceDraftDefinitionValidator.Id(skill.SkillId);
            if (!Enum.IsDefined(skill.Slot) || skill.Priority < 0) throw new ArgumentException("敌人技能方案或优先级无效。");
        }
        ResonanceDraftDefinitionValidator.Ids(value.InitialBuffs);
        if (value.InitialBuffs.Count > 256) throw new ArgumentException("初始 BUFF 最多 256 项。");
    }

    public static void References(EnemyDefinition value, ResonanceConfiguration configuration)
    {
        if (configuration.CombatRelations?.Species.Any(s => s.Id == value.SpeciesId) != true || configuration.CombatRelations.Factions.Any(f => f.Id == value.FactionId) != true)
            throw new ArgumentException("敌人引用了未登记种族或阵营。");
        foreach (var reference in value.Skills)
            if (!configuration.Skills.Any(s => s.Id == reference.SkillId && s.Variants.Any(v => v.Slot == reference.Slot)))
                throw new ArgumentException("敌人引用了不存在的谐振技能或槽位方案。");
        foreach (var id in value.InitialBuffs)
        {
            var buff = configuration.Buffs.SingleOrDefault(b => b.Id == id);
            if (buff == null || buff.Kind != BuffKind.Periodic || buff.Effects.Any(e => e.Kind == BuffEffectKind.Modifier && (e.Stat <= 2 || e.Stat >= 7)))
                throw new ArgumentException("初始 BUFF 需为现有周期效果，且不能修正六维属性。");
        }
    }
}
