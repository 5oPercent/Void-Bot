using System.Text.Json.Serialization;
namespace Void.Bot.Message.Dto.ERA;

/// <summary>谐振技能编辑与发布管理数据，不参与局内运行模拟。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraResonanceDraftSettings
{
    /// <summary>通用战术谐振技能 ID，有序且不能重复。</summary>
    public List<string> CommonTacticalSkills { get; set; } = new();

    /// <summary>可发布的种族、阵营和默认关系；旧目录可为空。</summary>
    public CombatRelationConfigurationDto? CombatRelations { get; set; }

    /// <summary>禁用谐振技能 ID，不删除配装。</summary>
    public List<string> DisabledSkills { get; set; } = new();
}
