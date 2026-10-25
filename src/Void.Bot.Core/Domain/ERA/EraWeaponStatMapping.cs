using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>共享的武器属性映射配置，保存四组双端值；通过装备目录发布，实例不复制端点。</summary>
[Table("era_weapon_stat_mapping")]
public sealed class EraWeaponStatMapping : IEntity<Guid>
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    /// <summary>精准度 0 分的散布角，单位度。</summary>
    public double SpreadDegreesAtZero { get; set; }
    /// <summary>精准度 100 分的散布角，单位度。</summary>
    public double SpreadDegreesAtHundred { get; set; }
    /// <summary>稳定性 0 分的每发后坐幅度，单位度。</summary>
    public double RecoilDegreesAtZero { get; set; }
    /// <summary>稳定性 100 分的每发后坐幅度，单位度。</summary>
    public double RecoilDegreesAtHundred { get; set; }
    /// <summary>操控性 0 分的切枪就绪时间，单位秒。</summary>
    public double ReadySecondsAtZero { get; set; }
    /// <summary>操控性 100 分的切枪就绪时间，单位秒。</summary>
    public double ReadySecondsAtHundred { get; set; }
    /// <summary>操控性 0 分的瞄准时间，单位秒。</summary>
    public double AimSecondsAtZero { get; set; }
    /// <summary>操控性 100 分的瞄准时间，单位秒。</summary>
    public double AimSecondsAtHundred { get; set; }
}
