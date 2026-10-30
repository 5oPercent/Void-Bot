using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>瞄准能力标记；当前没有独立参数，启用后要求模板配置精度属性及散布端点。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponAimCapabilityDto
{

}
