using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>装填能力配置；实际装填时长由基础秒数除以最终装填倍率计算。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponReloadCapabilityDto
{
    /// <summary>装填方式，支持整匣装填 Magazine 和逐发装填 PerRound。</summary>
    public EraWeaponReloadMode Mode { get; set; }
    /// <summary>装填倍率为 1 时的基础时长，单位秒，必须为有限正数且不超过 3600。</summary>
    public double BaseSeconds { get; set; }
}
