using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

public class GetDestiny2RaidDungeonStatsRequest : IRequest, IRequestDestiny2ByPlayerNameCode, ILoadDestiny2Player
{
    public string DisplayName { get; set; } = string.Empty;

    public string DisplayNameCode { get; set; } = string.Empty;

    public BungieMembershipTypeEnum MembershipType { get; set; }
}

public class GetDestiny2RaidDungeonStatsResponse : VoidBotResponse<Destiny2RaidDungeonStatsDto>
{
}
