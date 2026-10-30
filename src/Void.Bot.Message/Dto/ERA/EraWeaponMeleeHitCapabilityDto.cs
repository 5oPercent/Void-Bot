using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>近战命中能力标记；动作与 Hit 碰撞体决定命中窗口和覆盖范围，穿透属性决定目标数量。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponMeleeHitCapabilityDto
{

}
