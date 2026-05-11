using Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Component;

public class Destiny2ComponentCharactersDto
{
    public Dictionary<string, Destiny2ComponentCharactersDataDto> Data { get; set; } = new();

    public Destiny2ComponentPrivacyEnum Privacy { get; set; }
}

public class Destiny2ComponentCharactersDataDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// 游玩平台ID
    /// </summary>
    public string MembershipId { get; set; } = string.Empty;

    /// <summary>
    /// 游玩平台类型
    /// </summary>
    public BungieMembershipTypeEnum MembershipType { get; set; }

    /// <summary>
    /// 最后游玩时间
    /// </summary>
    public DateTime? DateLastPlayed { get; set; }

    /// <summary>
    /// 最后一次游玩时长(分钟)
    /// </summary>
    public string? MinutesPlayedThisSession { get; set; }

    /// <summary>
    /// 总共游玩时长(分钟)
    /// </summary>
    public string? MinutesPlayedTotal { get; set; }

    /// <summary>
    /// 光等
    /// </summary>
    public int Light { get; set; }

    /// <summary>
    /// 种族
    /// </summary>
    public Destiny2RaceEnum RaceType { get; set; }

    /// <summary>
    /// 职业
    /// </summary>
    public Destiny2ClassEnum ClassType { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public Destiny2GenderEnum GenderType { get; set; }

    /// <summary>
    /// 徽章
    /// </summary>
    public string EmblemPath { get; set; } = string.Empty;

    /// <summary>
    /// 徽章背景
    /// </summary>
    public string EmblemBackgroundPath { get; set; } = string.Empty;

    /// <summary>
    /// 徽章颜色
    /// </summary>
    public Destiny2ManifestColorDefinitionDto EmblemColor { get; set; } = new();
}