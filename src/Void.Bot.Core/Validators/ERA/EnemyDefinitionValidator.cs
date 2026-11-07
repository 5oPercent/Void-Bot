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
        if (!Enum.IsDefined(value.Tier)) throw new ArgumentException("敌人等阶无效。");
        EraDataDefinitionValidator.Id(value.Id);
        AbilityExecutionContract.Key(value.PresentationKey, true);
        EraDataDefinitionValidator.Id(value.SpeciesId);
        EraDataDefinitionValidator.Id(value.FactionId);
        Number(value.BaseHealth, 1, 1000000000);
        Number(value.BaseProtection, 0, 1000000000);
        Number(value.WeakPointHealthMultiplier, 0, 10000); Number(value.WeakPointProtectionMultiplier, 0, 10000);
        var tiers = value.TierMultipliers;
        if (tiers == null) throw new ArgumentException("缺少敌人等阶倍率表。");
        foreach (var multipliers in new[] { tiers.Minor, tiers.Elite, tiers.Major, tiers.Ultra, tiers.Boss })
        {
            if (multipliers == null) throw new ArgumentException("敌人等阶倍率表必须包含五档。");
            Number(multipliers.Health, 0.001, 10000);
            Number(multipliers.Protection, 0, 10000); Number(multipliers.Damage, 0, 10000);
        }
        Number(value.WalkSpeed, 0, 1000); Number(value.RunSpeed, 0, 1000);
        Number(value.RecoveryDelaySeconds, 0, 86400); Number(value.HealthRecoveryPerSecond, 0, 1000000000); Number(value.ProtectionRecoveryPerSecond, 0, 1000000000);
        var control = value.ControlDefense;
        if (control == null) throw new ArgumentException("缺少常驻控制防护配置。");
        Number(control.StunResistance, 0, 1); Number(control.RootResistance, 0, 1); Number(control.SilenceResistance, 0, 1);
        var ai = value.Ai;
        if (ai == null || !Enum.IsDefined(ai.Aggression)) throw new ArgumentException("敌人攻击倾向无效。");
        Number(ai.PerceptionDistance, 0, 1000000);
        Number(ai.FieldOfView, 0, 360);
        Number(ai.PerceptionIntervalSeconds, 0.001, 86400);
        Number(ai.HearingDistance, 0, 1000000);
        Number(ai.NearSenseDistance, 0, 1000000);
        Number(ai.ReactionMinimumSeconds, 0, 86400);
        Number(ai.ReactionMaximumSeconds, 0, 86400);
        Number(ai.TargetMemorySeconds, 0, 86400);
        Number(ai.TargetEvaluationSeconds, 0.001, 86400);
        Number(ai.RecentDamageWindowSeconds, 0.001, 86400);
        Number(ai.DamageNormalization, 0.001, 1000000000);
        Number(ai.VisibilityWeight, 0, 10000);
        Number(ai.DistanceWeight, 0, 10000);
        Number(ai.RecentDamageWeight, 0, 10000);
        Number(ai.LowHealthWeight, 0, 10000);
        Number(ai.CurrentTargetWeight, 0, 10000);
        Number(ai.TargetSwitchRatio, 1, 100);
        Number(ai.ScoreJitter, 0, 0.25);
        Number(ai.StopDistance, 0, 1000000);
        Number(ai.StuckTimeoutSeconds, 0.001, 86400);
        Number(ai.PreferredDistanceMinimum, 0, 1000000);
        Number(ai.PreferredDistanceMaximum, 0, 1000000);
        Number(ai.MeleeDistance, 0, 1000000);
        Number(ai.RetreatDistance, 0, 1000000);
        Number(ai.TurnSpeedDegrees, 0.001, 36000);
        Number(ai.DistanceHysteresis, 0, 1000000);
        Number(ai.CoverPreference, 0, 1);
        Number(ai.FlankPreference, 0, 1);
        Number(ai.AttackPreference, 0, 1);
        Number(ai.DefensePreference, 0, 1);
        Number(ai.SupportPreference, 0, 1);
        if (ai.ReactionMaximumSeconds < ai.ReactionMinimumSeconds || ai.PreferredDistanceMaximum < ai.PreferredDistanceMinimum
            || ai.RetreatDistance > ai.PreferredDistanceMinimum)
            throw new ArgumentException("反应或距离区间无效，后撤距离不能超过偏好距离下限。");
        if (ai.VisibilityWeight + ai.DistanceWeight + ai.RecentDamageWeight + ai.LowHealthWeight + ai.CurrentTargetWeight <= 0)
            throw new ArgumentException("目标评分至少需要一个正权重。");
        if (ai.AttackPreference + ai.DefensePreference + ai.SupportPreference <= 0)
            throw new ArgumentException("至少需要一种有效动作偏好。");
        var attack = value.NormalAttack;
        if (attack == null || !Enum.IsDefined(attack.Mode)) throw new ArgumentException("普通攻击方式无效。");
        Number(attack.BaseDamage, 0, 1000000000);
        Number(attack.Distance, 0, 1000000);
        Number(attack.WindupSeconds, 0, 86400);
        Number(attack.BackswingSeconds, 0, 86400);
        Number(attack.CooldownSeconds, 0, 86400);
        Number(attack.FirstAttackMinimumSeconds, 0, 86400);
        Number(attack.FirstAttackMaximumSeconds, 0, 86400);
        Number(attack.AimErrorDegrees, 0, 90);
        Number(attack.SpreadDegrees, 0, 90);
        Number(attack.FirstAttackSpreadMultiplier, 0, 10);
        Number(attack.Weight, 0, 10000);
        if (attack.FirstAttackMaximumSeconds < attack.FirstAttackMinimumSeconds)
            throw new ArgumentException("首次攻击等待区间无效。");
        if (attack.Mode == EnemyAttackMode.Melee && (attack.AimErrorDegrees != 0 || attack.SpreadDegrees != 0 || attack.FirstAttackSpreadMultiplier != 1))
            throw new ArgumentException("近战不能配置直射误差或散布。");
        if (value.Skills == null || value.Skills.Count > 256 || value.Skills.Any(s => s == null) || value.Skills.Select(s => s.SkillId).Distinct(StringComparer.Ordinal).Count() != value.Skills.Count)
            throw new ArgumentException("敌人技能列表无效或重复。");
        foreach (var skill in value.Skills)
        {
            EraDataDefinitionValidator.Id(skill.SkillId);
            if (!Enum.IsDefined(skill.Slot) || skill.Priority < 0) throw new ArgumentException("敌人技能方案或优先级无效。");
            var decision = skill.Decision;
            if (decision == null || !Enum.IsDefined(decision.Target) || !Enum.IsDefined(decision.Token))
                throw new ArgumentException("动作目标或名额类别无效。");
            Number(decision.MinimumDistance, 0, 1000000); Number(decision.MaximumDistance, decision.MinimumDistance, 1000000);
            Number(decision.MinimumHealthRatio, 0, 1); Number(decision.MaximumHealthRatio, decision.MinimumHealthRatio, 1);
            Number(decision.MaximumTargetHealthRatio, 0, 1); Number(decision.Weight, 0, 10000);
            if ((decision.Target == EnemyActionTarget.Enemy) == (decision.Token == EnemyAttackToken.Support)
                || decision.Target == EnemyActionTarget.Self && (decision.MinimumDistance != 0 || decision.MaximumDistance != 0 || decision.RequireLineOfSight))
                throw new ArgumentException("动作目标与名额、距离或视线条件不一致。");
        }
        var defense = value.Defense;
        if (defense == null || defense.SkillId == null) throw new ArgumentException("缺少防御配置。");
        Number(defense.EnterHealthRatio, 0, 1); Number(defense.ExitHealthRatio, defense.EnterHealthRatio, 1);
        if (defense.SkillId.Length > 0 && !value.Skills.Any(s => s.SkillId == defense.SkillId && s.Decision.Target == EnemyActionTarget.Self))
            throw new ArgumentException("防御动作必须引用当前技能列表中的自我目标动作。");
        EraDataDefinitionValidator.Ids(value.InitialBuffs);
        if (value.InitialBuffs.Count > 256) throw new ArgumentException("初始 BUFF 最多 256 项。");
    }

    public static void References(EnemyDefinition value, ResonanceConfiguration configuration)
    {
        if (configuration.CombatRelations?.Species.Any(s => s.Id == value.SpeciesId) != true || configuration.CombatRelations.Factions.Any(f => f.Id == value.FactionId) != true)
            throw new ArgumentException("敌人引用了未登记种族或阵营。");
        foreach (var reference in value.Skills)
        {
            var variant = configuration.Skills.FirstOrDefault(s => s.Id == reference.SkillId)?.Variants.FirstOrDefault(v => v.Slot == reference.Slot);
            if (variant == null) throw new ArgumentException("敌人引用了不存在的谐振技能或槽位方案。");
            if (variant.CarrierKind == ResonanceCarrierKind.Self && reference.Decision.Target != EnemyActionTarget.Self)
                throw new ArgumentException("自身载体必须使用自我目标动作。");
        }
        foreach (var id in value.InitialBuffs)
        {
            var buff = configuration.Buffs.SingleOrDefault(b => b.Id == id);
            if (buff == null || buff.Kind != BuffKind.Periodic || buff.Effects.Any(e => e.Kind == BuffEffectKind.Modifier && (e.Stat <= 2 || e.Stat >= 7)))
                throw new ArgumentException("初始 BUFF 需为现有周期效果，且不能修正六维属性。");
        }
    }
}
