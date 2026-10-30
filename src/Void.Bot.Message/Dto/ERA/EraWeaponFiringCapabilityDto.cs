using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>攻击触发能力配置，定义半自动、全自动、连发或蓄力释放方式。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponFiringCapabilityDto
{
    /// <summary>攻击触发模式；Charged 必须与 Charge 能力同时配置。</summary>
    public EraWeaponFireMode Mode { get; set; }
    /// <summary>一次连发的攻击次数，范围 1～64，默认 1；非 Burst 模式必须为 1。</summary>
    public int BurstCount { get; set; } = 1;
}
