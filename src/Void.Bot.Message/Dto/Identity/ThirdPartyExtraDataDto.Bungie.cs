using Void.Bot.Message.Dto.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Identity;

public class ThirdPartyBungieExtraDataDto
{
    public List<Destiny2LinkedProfileItemDto> Profiles { get; set; } = new();
}