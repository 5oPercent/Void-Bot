using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>一项武器属性修正；固定加值先累计，再应用累计百分比，不修改属性效果端点。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponModifierDto
{
    /// <summary>修正的目标属性；关联模板必须支持此属性，不包含已移除的射程。</summary>
    public EraWeaponStat Stat { get; set; }
    /// <summary>修正方式：Flat 为固定加值，AddPercent 为加算百分比。</summary>
    public EraWeaponModifierOperation Operation { get; set; }
    /// <summary>修正值，必须有限且绝对值不超过 1000000；Flat 使用属性单位，AddPercent 使用小数且必须大于 -1。</summary>
    public double Value { get; set; }
}
