using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_template")]
public sealed class EraWeaponTemplate : IEntity<Guid>
{
    [Key] public Guid Id { get; set; }
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    [StringLength(256)] public string Icon { get; set; } = string.Empty;
    [StringLength(2048)] public string Note { get; set; } = string.Empty;
    public bool IsGenerationEnabled { get; set; }
    [StringLength(128)] public string PresentationKey { get; set; } = string.Empty;
    /// <summary>共享属性映射配置的 Guid；随当前目录读取，不复制端点到实例。</summary>
    public Guid StatMappingId { get; set; }
    /// <summary>单发基础伤害；角色强度、BUFF 与目标结算由游戏权威端另行应用。</summary>
    public double Damage { get; set; }
    /// <summary>射击频率，单位次／秒；例如 10 表示每秒 10 发，即 600 RPM。</summary>
    public double Frequency { get; set; }
    /// <summary>弹匣装载上限，单位发，正整数；不表示当前已装载量。</summary>
    public int LoadedCapacity { get; set; }
    /// <summary>每把武器独立的储备上限，单位发，非负整数；不与其他武器共享。</summary>
    public int ReserveCapacity { get; set; }
    /// <summary>装填速度系数，1 为基础速度，1.2 表示速度提高 20%；装填秒数 = BaseReloadSeconds / ReloadSpeed。</summary>
    public double ReloadSpeed { get; set; }
    /// <summary>射程，单位米；距离对应的实际效果由游戏端规则解释。</summary>
    public double Range { get; set; }
    /// <summary>精准度基础评分，范围 0～100，非百分比；结合改装后的评分按关联效果配置换算为散布角。</summary>
    public double Accuracy { get; set; }
    /// <summary>稳定性基础评分，范围 0～100，非百分比；结合改装后的评分按关联效果配置换算为后坐幅度。</summary>
    public double Stability { get; set; }
    /// <summary>操控性基础评分，范围 0～100，非百分比；结合改装后的评分按关联效果配置换算为就绪、瞄准耗时。</summary>
    public double Handling { get; set; }
    /// <summary>投射物速度，单位米／秒，仅飞行投射物有值；即时射线为 null。</summary>
    public double? ProjectileSpeed { get; set; }
    /// <summary>完整蓄力时间，单位秒，仅蓄力开火有值；当前全自动步枪为 null。</summary>
    public double? ChargeTime { get; set; }
    /// <summary>类型化行为配置 JSON，包含射击／命中／装填模式及基础装填秒数；评分效果端点保存在关联效果配置。</summary>
    [Column(TypeName = "text")] public string BehaviorPayload { get; set; } = string.Empty;
    [Column(TypeName = "text")] public string CompatibleSlotsPayload { get; set; } = string.Empty;
}
