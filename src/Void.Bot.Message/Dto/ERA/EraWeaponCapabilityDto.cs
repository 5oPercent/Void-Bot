using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>框架的一项能力及其类型化配置；必须且只能填写与 Kind 对应的一个配置对象。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponCapabilityDto
{
    /// <summary>能力类型，决定必须填写哪一个配置对象。</summary>
    public EraWeaponCapabilityKind Kind { get; set; }
    /// <summary>瞄准能力标记；Kind 为 Aim 时必须非空，其他类型为空。</summary>
    public EraWeaponAimCapabilityDto? Aim { get; set; }
    /// <summary>后坐能力标记；Kind 为 Recoil 时必须非空，其他类型为空。</summary>
    public EraWeaponRecoilCapabilityDto? Recoil { get; set; }
    /// <summary>装填方式与基础时长；Kind 为 Reload 时必须非空，其他类型为空。</summary>
    public EraWeaponReloadCapabilityDto? Reload { get; set; }
    /// <summary>蓄力释放规则；Kind 为 Charge 时必须非空，其他类型为空。</summary>
    public EraWeaponChargeCapabilityDto? Charge { get; set; }
    /// <summary>投射物最大飞行距离；Kind 为 Projectile 时必须非空，其他类型为空。</summary>
    public EraWeaponProjectileCapabilityDto? Projectile { get; set; }
    /// <summary>近战 Hit 命中能力标记；Kind 为 MeleeHit 时必须非空，其他类型为空。</summary>
    public EraWeaponMeleeHitCapabilityDto? MeleeHit { get; set; }
    /// <summary>攻击触发方式与连发次数；Kind 为 Firing 时必须非空，其他类型为空。</summary>
    public EraWeaponFiringCapabilityDto? Firing { get; set; }
    /// <summary>射线最大检测距离；Kind 为 Hitscan 时必须非空，其他类型为空。</summary>
    public EraWeaponHitscanCapabilityDto? Hitscan { get; set; }
}
