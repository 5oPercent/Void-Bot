using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Message.Commands.Bungie.Destiny2;

public class GetDestiny2RaidDungeonStatsCommand : ICommand, IRequestDestiny2ByPlayerNameCode, ILoadDestiny2Player
{
    public string DisplayName { get; set; } = string.Empty;

    public string DisplayNameCode { get; set; } = string.Empty;

    public BungieMembershipTypeEnum MembershipType { get; set; }
}

public class GetDestiny2RaidDungeonStatsResponse : VoidBotResponse<Destiny2RaidDungeonStatsDto>
{
}
