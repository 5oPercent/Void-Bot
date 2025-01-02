using Void.Bot.Message.Request.Bungie.Domain;

namespace Void.Bot.Message.Dto.Identity;

public class ThirdPartyBungieExtraDataDto
{
    public List<BungieApiLinkedProfileItemDto> Profiles { get; set; } = new();
}