using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>射线命中能力配置；最大检测距离属于框架行为参数，不是可改装属性。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponHitscanCapabilityDto
{
    /// <summary>射线最大检测距离，单位米，必须为有限正数且不超过 1000000；不代表穿透目标数量。</summary>
    public double MaximumDistance { get; set; }
}
