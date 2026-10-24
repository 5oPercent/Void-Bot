using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>可被多个模板引用的评分效果配置；端点与武器目录一起发布。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponStatEffectProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
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
