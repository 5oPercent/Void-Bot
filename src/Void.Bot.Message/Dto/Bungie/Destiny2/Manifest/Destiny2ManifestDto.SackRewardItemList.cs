namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 战利品奖励物品列表定义，描述游戏中从特定战利品袋中可能获得的物品集合
/// </summary>
public class Destiny2ManifestSackRewardItemListDefinitionDto : Destiny2ManifestDefinitionDto
{
}

/// <summary>
/// 战利品奖励物品列表清单，存储所有战利品袋奖励物品列表定义的字典集合
/// </summary>
public class Destiny2ManifestSackRewardItemListDto : Dictionary<string, Destiny2ManifestSackRewardItemListDefinitionDto>
{
}