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
    public EnemyAiDefinition Ai { get; set; } = new();
    public EnemyNormalAttackDefinition NormalAttack { get; set; } = new();
    public List<EnemySkillReference> Skills { get; set; } = new();
    public List<string> InitialBuffs { get; set; } = new();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyAiDefinition
{
    public EnemyBehavior Behavior { get; set; }
    public EnemyAggression Aggression { get; set; }
    public double PerceptionDistance { get; set; } = 18;
    public double FieldOfView { get; set; } = 120;
    public double PerceptionIntervalSeconds { get; set; } = 0.3;
    public double TargetMemorySeconds { get; set; } = 6;
    public double MaximumChaseSeconds { get; set; } = 20;
    public double StopDistance { get; set; } = 1.8;
    public double StuckTimeoutSeconds { get; set; } = 3;
    public double IdleMinimumSeconds { get; set; } = 2;
    public double IdleMaximumSeconds { get; set; } = 5;
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyNormalAttackDefinition
{
    /// <summary>基础伤害；普通攻击伤害为基础伤害乘当前等阶伤害倍率后向上取整，默认 10。</summary>
    public double BaseDamage { get; set; } = 10;
    public double Distance { get; set; } = 1.8;
    public double WindupSeconds { get; set; } = 0.5;
    public double BackswingSeconds { get; set; } = 0.6;
    public double CooldownSeconds { get; set; } = 1.8;
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

/// <summary>指定已有技能的槽位方案；优先级大者优先，同级按配置顺序。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemySkillReference
{
    public string SkillId { get; set; } = "";
    public ResonanceSlotKind Slot { get; set; } = ResonanceSlotKind.Primary;
    public int Priority { get; set; } = 10;
}
