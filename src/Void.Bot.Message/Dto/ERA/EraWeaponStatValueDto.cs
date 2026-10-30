using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>模板的一项基础属性及其可选评分端点；整数、特殊值和端点规则按 Stat 校验。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponStatValueDto
{
    /// <summary>属性标识，JSON 必填；同一模板中不得重复，且必须属于框架的适用属性集合。</summary>
    [JsonRequired]
    public EraWeaponStat Stat { get; set; }
    /// <summary>属性基础值，JSON 必填且必须为有限数；单位及范围按 Stat 校验，储备和穿透允许 -1，容量始终有限。</summary>
    [JsonRequired]
    public double Value { get; set; }
    /// <summary>评分为 0 时的实际效果；精度／稳定分别为散布／后坐角度（0～180 度），操控为掏出时间（>0 且 ≤3600 秒）；三者须成对配置，其他属性为空。</summary>
    public double? EffectAtZero { get; set; }
    /// <summary>评分为 100 时的实际效果；单位及边界与 EffectAtZero 相同，必须成对配置且不大于其值；非评分属性为空。</summary>
    public double? EffectAtHundred { get; set; }
}
