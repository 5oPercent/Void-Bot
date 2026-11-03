using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>给予已有的效果配置；空条件继承默认触发条件，非空条件完整覆盖，均不限制释放；自身载体不判断条件。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResonanceGrantedEffectDto
{
    public string EffectId { get; set; } = string.Empty;
    public ResonanceEntityConditionDto? Condition { get; set; }
}
