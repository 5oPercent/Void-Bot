using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;

namespace Void.Bot.Message.Request.Sync;

public class GetDestiny2ManifestSyncOptionsRequest : IRequest
{
}

public class GetDestiny2ManifestSyncOptionsResponse : VoidBotResponse<Destiny2ManifestSyncOptionsDto>
{
}
