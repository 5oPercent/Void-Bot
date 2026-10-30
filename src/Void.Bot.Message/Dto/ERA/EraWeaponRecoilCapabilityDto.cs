using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>后坐能力标记；当前没有独立参数，后坐角度由模板稳定属性的效果端点决定。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponRecoilCapabilityDto
{

}
