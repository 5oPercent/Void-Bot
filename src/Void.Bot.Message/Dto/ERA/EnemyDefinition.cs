using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>敌人模板；不含六维、地图生成规则和实例运行状态。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>敌人等阶，默认普通；决定当前倍率行与客户端 HUD 表现。</summary>
    public EnemyTier Tier { get; set; } = EnemyTier.Minor;
    public string Description { get; set; } = "";
    /// <summary>客户端表现资源数据集 Key；空字符串时按敌人 Id 解析，保存时保留空值。</summary>
    public string PresentationKey { get; set; } = "";
    public string SpeciesId { get; set; } = "";
    public string FactionId { get; set; } = "";
    /// <summary>基础生命；生命上限为基础生命乘当前等阶生命倍率后向上取整，默认 100。</summary>
    public double BaseHealth { get; set; } = 100;
    /// <summary>基础防护；防护上限为基础防护乘当前等阶防护倍率后向上取整，默认 50。</summary>
    public double BaseProtection { get; set; } = 50;
    /// <summary>本敌人独立的五档倍率；切换等阶不改写基础值或其他档位。</summary>
    public EnemyTierMultipliersDefinition TierMultipliers { get; set; } = new();
    public double WalkSpeed { get; set; } = 2;
    public double RunSpeed { get; set; } = 4;
    /// <summary>最近受伤后的恢复延迟，单位为秒；是否执行恢复由客户端决定。</summary>
    public double RecoveryDelaySeconds { get; set; } = 5;
    /// <summary>每秒生命恢复量，与生命、防护及移动数值一起配置。</summary>
    public double HealthRecoveryPerSecond { get; set; }
    /// <summary>每秒防护恢复量。</summary>
    public double ProtectionRecoveryPerSecond { get; set; }
    public EnemyControlDefenseDefinition ControlDefense { get; set; } = new();
    public EnemyAiDefinition Ai { get; set; } = new();
    public EnemyNormalAttackDefinition NormalAttack { get; set; } = new();
    public EnemyDefenseDefinition Defense { get; set; } = new();
    public List<EnemySkillReference> Skills { get; set; } = new();
    public List<string> InitialBuffs { get; set; } = new();
}

/// <summary>常驻控制防护；抗性缩短对应控制时长，范围 0～1，不随等阶倍率缩放。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyControlDefenseDefinition
{
    public double StunResistance { get; set; }
    public double RootResistance { get; set; }
    public double SilenceResistance { get; set; }
    public bool StunImmunity { get; set; }
    public bool RootImmunity { get; set; }
    public bool SilenceImmunity { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyAiDefinition
{
    public EnemyAggression Aggression { get; set; }
    /// <summary>视觉距离（m）。</summary>
    public double PerceptionDistance { get; set; } = 18;
    /// <summary>视野角（°）。</summary>
    public double FieldOfView { get; set; } = 120;
    /// <summary>感知间隔（s）。</summary>
    public double PerceptionIntervalSeconds { get; set; } = 0.3;
    /// <summary>战斗声感知距离（m）。</summary>
    public double HearingDistance { get; set; } = 18;
    /// <summary>近距全向视觉（m）。</summary>
    public double NearSenseDistance { get; set; } = 2;
    /// <summary>最短反应延迟（s）。</summary>
    public double ReactionMinimumSeconds { get; set; } = 0.3;
    /// <summary>最长反应延迟（s）。</summary>
    public double ReactionMaximumSeconds { get; set; } = 0.6;
    /// <summary>目标记忆（s）。</summary>
    public double TargetMemorySeconds { get; set; } = 6;
    /// <summary>目标重评间隔（s）。</summary>
    public double TargetEvaluationSeconds { get; set; } = 0.5;
    /// <summary>近期伤害窗口（s）。</summary>
    public double RecentDamageWindowSeconds { get; set; } = 3;
    /// <summary>伤害归一化尺度。</summary>
    public double DamageNormalization { get; set; } = 100;
    /// <summary>可见性权重。</summary>
    public double VisibilityWeight { get; set; } = 0.3;
    /// <summary>距离权重。</summary>
    public double DistanceWeight { get; set; } = 0.25;
    /// <summary>近期伤害权重。</summary>
    public double RecentDamageWeight { get; set; } = 0.2;
    /// <summary>低生命权重。</summary>
    public double LowHealthWeight { get; set; } = 0.05;
    /// <summary>当前目标权重。</summary>
    public double CurrentTargetWeight { get; set; } = 0.05;
    /// <summary>切换评分倍率。</summary>
    public double TargetSwitchRatio { get; set; } = 1.2;
    /// <summary>评分扰动比例。</summary>
    public double ScoreJitter { get; set; } = 0.1;
    /// <summary>到位容差（m）。</summary>
    public double StopDistance { get; set; } = 0.5;
    /// <summary>受阻重选时间（s）。</summary>
    public double StuckTimeoutSeconds { get; set; } = 3;
    /// <summary>偏好距离下限（m）。</summary>
    public double PreferredDistanceMinimum { get; set; } = 0;
    /// <summary>偏好距离上限（m）。</summary>
    public double PreferredDistanceMaximum { get; set; } = 1.6;
    /// <summary>近战触发距离（m）。</summary>
    public double MeleeDistance { get; set; } = 1.8;
    /// <summary>后撤触发距离（m）。</summary>
    public double RetreatDistance { get; set; } = 0;
    /// <summary>转向速度（°/s）。</summary>
    public double TurnSpeedDegrees { get; set; } = 180;
    /// <summary>距离切换迟滞（m）。</summary>
    public double DistanceHysteresis { get; set; } = 0.2;
    /// <summary>掩体偏好。</summary>
    public double CoverPreference { get; set; } = 0;
    /// <summary>绕侧偏好。</summary>
    public double FlankPreference { get; set; } = 0;
    /// <summary>进攻偏好。</summary>
    public double AttackPreference { get; set; } = 1;
    /// <summary>防御偏好。</summary>
    public double DefensePreference { get; set; } = 1;
    /// <summary>支援偏好。</summary>
    public double SupportPreference { get; set; } = 1;
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyNormalAttackDefinition
{
    public EnemyAttackMode Mode { get; set; }
    /// <summary>基础伤害。</summary>
    public double BaseDamage { get; set; } = 10;
    /// <summary>攻击距离（m）。</summary>
    public double Distance { get; set; } = 1.8;
    /// <summary>前摇（s）。</summary>
    public double WindupSeconds { get; set; } = 0.5;
    /// <summary>后摇（s）。</summary>
    public double BackswingSeconds { get; set; } = 0.6;
    /// <summary>冷却（s）。</summary>
    public double CooldownSeconds { get; set; } = 1.8;
    /// <summary>首次攻击最短等待（s）。</summary>
    public double FirstAttackMinimumSeconds { get; set; } = 0.4;
    /// <summary>首次攻击最长等待（s）。</summary>
    public double FirstAttackMaximumSeconds { get; set; } = 0.7;
    /// <summary>瞄准误差（°）。</summary>
    public double AimErrorDegrees { get; set; } = 0;
    /// <summary>直射散布（°）。</summary>
    public double SpreadDegrees { get; set; } = 0;
    /// <summary>首轮散布倍率。</summary>
    public double FirstAttackSpreadMultiplier { get; set; } = 1;
    /// <summary>动作评分权重。</summary>
    public double Weight { get; set; } = 1;
}

/// <summary>本敌人的五档独立倍率；生命默认 1/5/10/20/100，防护 0/3/6/10/0，伤害 1/2/3/4/2。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyTierMultipliersDefinition
{
    [JsonRequired]
    public EnemyStatMultipliersDefinition Minor { get; set; } = new() { Health = 1, Protection = 0, Damage = 1 };
    [JsonRequired]
    public EnemyStatMultipliersDefinition Elite { get; set; } = new() { Health = 5, Protection = 3, Damage = 2 };
    [JsonRequired]
    public EnemyStatMultipliersDefinition Major { get; set; } = new() { Health = 10, Protection = 6, Damage = 3 };
    [JsonRequired]
    public EnemyStatMultipliersDefinition Ultra { get; set; } = new() { Health = 20, Protection = 10, Damage = 4 };
    [JsonRequired]
    public EnemyStatMultipliersDefinition Boss { get; set; } = new() { Health = 100, Protection = 0, Damage = 2 };
}

/// <summary>单档生命、防护及普通攻击伤害的独立倍率；最终值统一向上取整。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyStatMultipliersDefinition
{
    /// <summary>生命倍率，必须大于 0。</summary>
    [JsonRequired]
    public double Health { get; set; } = 1;
    /// <summary>防护倍率，允许为 0。</summary>
    [JsonRequired]
    public double Protection { get; set; } = 1;
    /// <summary>普通攻击伤害倍率，允许为 0；不隐含能力启停，也不修改引用技能的效果数值。</summary>
    [JsonRequired]
    public double Damage { get; set; } = 1;
}

/// <summary>一个技能一个执行实例；SkillId 是稳定去重键，Priority 仅用于同分排序。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemySkillReference
{
    public string SkillId { get; set; } = "";
    public ResonanceSlotKind Slot { get; set; } = ResonanceSlotKind.Primary;
    public int Priority { get; set; } = 10;
    public EnemyActionDecision Decision { get; set; } = new();
}

/// <summary>全部条件同时满足后才参与评分；效果、释放过程和冷却沿用技能定义。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyActionDecision
{
    public EnemyActionTarget Target { get; set; }
    public double MinimumDistance { get; set; }
    public double MaximumDistance { get; set; } = 30;
    public bool RequireLineOfSight { get; set; } = true;
    public double MinimumHealthRatio { get; set; }
    public double MaximumHealthRatio { get; set; } = 1;
    public double MaximumTargetHealthRatio { get; set; } = 1;
    public double Weight { get; set; } = 1;
    public bool InterruptOnTargetLost { get; set; } = true;
    public bool OncePerEngagement { get; set; }
    public EnemyAttackToken Token { get; set; } = EnemyAttackToken.Ranged;
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyDefenseDefinition
{
    /// <summary>引用 Skills 中的自我目标动作；空值表示未配置。</summary>
    public string SkillId { get; set; } = "";
    public double EnterHealthRatio { get; set; } = 0.33;
    public double ExitHealthRatio { get; set; } = 0.5;
    public bool OncePerEngagement { get; set; } = true;
}
