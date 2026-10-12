namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 整批发布的技能配置，包含能力体系、技能方案、BUFF 定义和禁用列表，不包含客户端表现资源。
/// </summary>
public sealed class SkillConfiguration
{
    /// <summary>
    /// 整份配置或配装的版本，由后端发布／保存时递增。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 配置数据结构版本；当前为 1，用于识别客户端支持的契约。
    /// </summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// 全部能力体系定义；客户端再依据玩家状态进行展示。
    /// </summary>
    public List<AbilitySystemDefinition> Systems { get; set; } = new List<AbilitySystemDefinition>();

    /// <summary>
    /// 完整技能定义列表，包含各技能支持的槽位方案。
    /// </summary>
    public List<SkillDefinition> Skills { get; set; } = new List<SkillDefinition>();

    /// <summary>
    /// 技能方案可以引用的完整 BUFF 定义列表。
    /// </summary>
    public List<BuffDefinitionDto> Buffs { get; set; } = new List<BuffDefinitionDto>();

    /// <summary>
    /// 所有体系共享的通用战术技能 ID，选择时与体系专属池合并去重。
    /// </summary>
    public List<string> CommonTacticalSkills { get; set; } = new List<string>();

    /// <summary>
    /// 本次配置中禁用的技能 ID；禁用不阻止配装保存，由主机禁止局内释放。
    /// </summary>
    public List<string> DisabledSkills { get; set; } = new List<string>();
}
