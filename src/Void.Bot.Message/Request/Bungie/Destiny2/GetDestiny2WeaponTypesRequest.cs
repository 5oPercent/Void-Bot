using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public class GetDestiny2WeaponTypesRequest : IRequest
{
    public string Language { get; set; } = string.Empty;
}

public class GetDestiny2WeaponTypesResponse : VoidBotResponse<Destiny2WeaponTypesDto>
{
}
