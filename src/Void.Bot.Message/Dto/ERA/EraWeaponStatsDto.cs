using System.Text.Json.Serialization;
namespace Void.Bot.Message.Dto.ERA;

/// <summary>公共模板基础值；不保存为武器实例属性。频率为次/秒，距离为米，时间为秒。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponStatsDto
{
    public double Damage { get; set; }
    public double Frequency { get; set; }
    public int LoadedCapacity { get; set; }
    public int ReserveCapacity { get; set; }
    public double ReloadSpeed { get; set; }
    public double Range { get; set; }
    public double Accuracy { get; set; }
    public double Stability { get; set; }
    public double Handling { get; set; }
    public double? ProjectileSpeed { get; set; }
    public double? ChargeTime { get; set; }
}
