namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWorldMemberDto
{
    /// <summary>ERA 玩家身份，不是主账号 ID。</summary>
    public Guid PlayerId { get; set; }
    /// <summary>该成员加入世界时固定的角色身份。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>与既有队伍绑定的成员身份。</summary>
    public Guid MemberId { get; set; }
    /// <summary>本人报告离开或世界结束后为假，不由一次心跳失败推断。</summary>
    public bool IsContinuing { get; set; }
}
