using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponModifierDto
{
    public EraWeaponStat Stat { get; set; }
    public EraWeaponModifierOperation Operation { get; set; }
    public double Value { get; set; }
}
