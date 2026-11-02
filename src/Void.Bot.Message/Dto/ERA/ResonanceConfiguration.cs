using System.Text.Json.Serialization;
namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 整批发布的谐振技能配置，包含晶格体系、谐振技能方案、BUFF 定义和禁用列表，不包含客户端表现资源。
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResonanceConfiguration
{
    /// <summary>
    /// 整份配置或配装的版本，由后端发布／保存时递增。
    /// </summary>
    public long Version { get; set; }

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

    /// <summary>
    /// 所有晶格体系共享的通用战术谐振技能 ID，选择时与晶格体系专属池合并去重。
    /// </summary>
    public List<string> CommonTacticalSkills { get; set; } = new List<string>();

    /// <summary>可发布的种族、阵营和默认关系；旧目录可为空。</summary>
    public CombatRelationConfigurationDto? CombatRelations { get; set; }

    /// <summary>
    /// 本次配置中禁用的谐振技能 ID；禁用不阻止配装保存，由主机禁止局内释放。
    /// </summary>
    public List<string> DisabledSkills { get; set; } = new List<string>();
}
