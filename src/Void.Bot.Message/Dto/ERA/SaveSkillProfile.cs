namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 保存配装的传输数据，预期版本用于防止旧数据覆盖新的配装。
/// </summary>
public sealed class SaveSkillProfile
{
    /// <summary>
    /// 读取时取得的配装版本，用于保存时进行并发冲突校验。
    /// </summary>
    public long ExpectedVersion { get; set; }

    /// <summary>
    /// 待保存的完整配装档案，包含当前体系选择与各体系记录。
    /// </summary>
    public PlayerSkillProfile Profile { get; set; } = new PlayerSkillProfile();
}
