using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>飞行投射物能力配置；飞行速度由模板的投射物速度属性提供。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponProjectileCapabilityDto
{
    /// <summary>投射物最大飞行距离，单位米，必须为有限正数且不超过 1000000；不代表穿透目标数量。</summary>
    public double MaximumDistance { get; set; }
}
