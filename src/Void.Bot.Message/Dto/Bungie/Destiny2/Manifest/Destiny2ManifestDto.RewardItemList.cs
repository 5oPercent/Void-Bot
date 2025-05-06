namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 奖励物品列表定义，描述游戏中可获得的一组奖励物品集合
/// </summary>
public class Destiny2ManifestRewardItemListDefinitionDto : Destiny2ManifestDefinitionDto
{
}

/// <summary>
/// 奖励物品列表清单，存储所有奖励物品列表定义的字典集合
/// </summary>
public class Destiny2ManifestRewardItemListDto : Dictionary<string, Destiny2ManifestRewardItemListDefinitionDto>
{
}