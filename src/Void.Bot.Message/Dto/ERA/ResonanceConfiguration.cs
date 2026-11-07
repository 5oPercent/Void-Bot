using System.Text.Json.Serialization;
namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 从独立业务表组合的当前配置，包含晶格体系、谐振技能方案、BUFF 定义及种族阵营关系，不包含客户端表现资源。
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResonanceConfiguration
{
    /// <summary>
    /// 配置数据结构版本；当前为 3，用于识别客户端支持的契约。
    /// </summary>
    public int SchemaVersion { get; set; } = 3;

    /// <summary>
    /// 全部晶格体系定义；客户端再依据玩家状态进行展示。
    /// </summary>
    public List<LatticeDefinition> Systems { get; set; } = new List<LatticeDefinition>();

    /// <summary>
    /// 完整谐振技能定义列表，包含各谐振技能支持的槽位方案。
    /// </summary>
    public List<ResonanceDefinition> Skills { get; set; } = new List<ResonanceDefinition>();

    /// <summary>
    /// 谐振技能方案可以引用的完整 BUFF 定义列表。
    /// </summary>
    public List<BuffDefinitionDto> Buffs { get; set; } = new List<BuffDefinitionDto>();

    /// <summary>敌人模板集合；游戏目录在 IncludeEnemies 请求时输出，旧客户端缺省不携带。</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<EnemyDefinition>? Enemies { get; set; }

    /// <summary>当前种族、阵营和默认关系；旧目录可为空。</summary>
    public CombatRelationConfigurationDto? CombatRelations { get; set; }

}
