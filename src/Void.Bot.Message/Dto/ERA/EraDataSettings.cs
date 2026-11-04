using System.Text.Json.Serialization;
namespace Void.Bot.Message.Dto.ERA;

/// <summary>种族阵营关系的编辑数据，业务内容保存在独立关系表。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraDataSettings
{
    /// <summary>当前种族、阵营和默认关系。</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CombatRelationConfigurationDto? CombatRelations { get; set; }

}
