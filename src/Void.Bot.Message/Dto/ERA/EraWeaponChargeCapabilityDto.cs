using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>蓄力能力配置；完整蓄力时长由模板的蓄力时间属性提供。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponChargeCapabilityDto
{
    /// <summary>是否允许未满蓄力时提前释放，默认不允许；不定义提前释放的伤害曲线。</summary>
    public bool AllowPartialRelease { get; set; }
}
