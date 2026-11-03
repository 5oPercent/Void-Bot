using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>敌人模板；不含六维、地图生成规则和实例运行状态。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string PresentationKey { get; set; } = "";
    public string SpeciesId { get; set; } = "";
    public string FactionId { get; set; } = "";
    public double MaximumHealth { get; set; } = 100;
    public double MaximumProtection { get; set; }
    public double WalkSpeed { get; set; } = 2;
    public double RunSpeed { get; set; } = 4;
    public EnemyAiDefinition Ai { get; set; } = new();
    public EnemyNormalAttackDefinition NormalAttack { get; set; } = new();
    public List<EnemySkillReference> Skills { get; set; } = new();
    public List<string> InitialBuffs { get; set; } = new();
    public EnemyRecoveryDefinition Recovery { get; set; } = new();
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
    public bool Enabled { get; set; }
    public double Damage { get; set; }
    public double Distance { get; set; } = 1.8;
    public double WindupSeconds { get; set; } = 0.5;
    public double BackswingSeconds { get; set; } = 0.6;
    public double CooldownSeconds { get; set; } = 1.8;
}

/// <summary>指定已有技能的槽位方案；优先级大者优先，同级按配置顺序。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemySkillReference
{
    public string SkillId { get; set; } = "";
    public ResonanceSlotKind Slot { get; set; } = ResonanceSlotKind.Primary;
    public int Priority { get; set; } = 10;
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnemyRecoveryDefinition
{
    public bool HealthEnabled { get; set; }
    public double DelaySeconds { get; set; } = 5;
    public double HealthPerSecond { get; set; }
    public double ProtectionPerSecond { get; set; }
}
