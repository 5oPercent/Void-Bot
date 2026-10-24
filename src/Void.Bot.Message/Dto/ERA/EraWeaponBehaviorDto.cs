using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponBehaviorDto
{
    public EraWeaponFireMode FireMode { get; set; }
    public EraWeaponHitMode HitMode { get; set; }
    public EraWeaponReloadMode ReloadMode { get; set; }
    /// <summary>基础装填时长，单位秒；实际装填时长 = BaseReloadSeconds / 最终 ReloadSpeed。</summary>
    public double BaseReloadSeconds { get; set; }
    public int BurstCount { get; set; } = 1;
}
