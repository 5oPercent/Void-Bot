namespace Void.Bot.Message.Dto.ERA;

/// <summary>历史生成回执；只含标识，不包含队友的完整装备属性，也不表示掉落当前仍可领取。</summary>
public sealed class EraGroundRewardReceiptDto
{
    public Guid EventId { get; set; }
    public Guid WorldInstanceId { get; set; }
    public long ConfigurationVersion { get; set; }
    public long ManifestVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<EraGroundRewardGrantReceiptDto> Grants { get; set; } = new List<EraGroundRewardGrantReceiptDto>();
}
