using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>给予已有的效果配置；空条件继承槽位默认条件，非空条件完整覆盖。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResonanceGrantedEffectDto
{
    public string EffectId { get; set; } = string.Empty;
    public ResonanceEntityConditionDto? Condition { get; set; }
}
