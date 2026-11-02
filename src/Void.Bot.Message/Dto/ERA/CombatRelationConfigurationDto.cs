using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>随正式目录发布的身份定义与默认关系；实体身份和临时关系仍由世界主机同步。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CombatRelationConfigurationDto
{
    public List<CombatIdentityDefinitionDto> Species { get; set; } = new();
    public List<CombatIdentityDefinitionDto> Factions { get; set; } = new();
    public List<CombatRelationRuleDto> FactionRules { get; set; } = new();
    public List<CombatRelationRuleDto> SpeciesRules { get; set; } = new();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CombatIdentityDefinitionDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CombatRelationRuleDto
{
    public string ObserverId { get; set; } = "";
    public string TargetId { get; set; } = "";
    /// <summary>Friendly=2、Neutral=3、Hostile=4；Self 和 Unknown 由运行时裁决。</summary>
    public int Relation { get; set; } = 3;
    public bool Bidirectional { get; set; } = true;
}
