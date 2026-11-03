using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>固定槽位方案；效果数值与生命周期只由效果配置定义。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResonanceVariant
{
    public ResonanceSlotKind Slot { get; set; }
    public int MaximumCharges { get; set; } = 1;
    public double CooldownSeconds { get; set; }
    public ResonanceRecoveryStart RecoveryStart { get; set; }
    public ResonanceCastMode CastMode { get; set; }
    /// <summary>-1 表示不限制距离，其他值为非负米数。</summary>
    public double MaximumDistance { get; set; } = 30;
    /// <summary>仅准备、预览及等待出手期间限制移动，出手后解除。</summary>
    public bool AllowMovement { get; set; } = true;
    public string ActionKey { get; set; } = string.Empty;
    public ResonanceCarrierKind CarrierKind { get; set; }
    /// <summary>仅控制构造物与区域的存在时间，不控制已给予效果的寿命。</summary>
    public double DurationSeconds { get; set; }
    public double TriggerIntervalSeconds { get; set; }
    /// <summary>从载体生成起计时，延迟启动载体自身逻辑；不推迟载体生成或结束。</summary>
    public double ExecutionDelaySeconds { get; set; }
    /// <summary>默认效果触发条件；不限制释放，单项效果可完整覆盖，自身载体不判断条件。</summary>
    public ResonanceEntityConditionDto Condition { get; set; } = new();
    public List<ResonanceGrantedEffectDto> Effects { get; set; } = new();
}
