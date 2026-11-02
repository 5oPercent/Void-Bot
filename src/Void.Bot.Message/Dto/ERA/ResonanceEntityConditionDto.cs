using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>固定种族／阵营条件。关系值为 1 自身、2 友方、3 中立、4 敌对。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResonanceEntityConditionDto
{
    public ResonanceConditionKind Kind { get; set; }
    public string IdentityId { get; set; } = string.Empty;
    public int Relation { get; set; }
    public List<ResonanceEntityConditionDto> Children { get; set; } = new();
}
