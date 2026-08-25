using Void.Bot.Message.Dto.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Bungie.Destiny2.RaidDungeon;

internal sealed class Destiny2CharacterActivity(string characterId, Destiny2Activity activity)
{
    public string CharacterId { get; } = characterId;

    public Destiny2Activity Activity { get; } = activity;
}
