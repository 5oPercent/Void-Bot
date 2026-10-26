using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraArmorModifierDto
{
    /// <summary>护甲六维属性标识，取值见 EraArmorAttribute。</summary>
    public EraArmorAttribute Stat { get; set; }
    /// <summary>固定加值或加算百分比；百分比用小数表示，0.2 表示增加 20%。</summary>
    public EraArmorModifierOperation Operation { get; set; }
    public double Value { get; set; }
}
