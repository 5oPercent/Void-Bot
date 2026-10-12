namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 玩家长期技能配装档案，保存当前体系与各体系配装，不保存局内冷却或载体状态。
/// </summary>
public sealed class PlayerSkillProfile
{
    /// <summary>
    /// 整份配置或配装的版本，由后端发布／保存时递增。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 玩家当前选择的能力体系 ID；null 表示尚未选择。
    /// </summary>
    public string? SelectedSystemId { get; set; }

    /// <summary>
    /// 每个体系最后保存的四槽配装，体系 ID 不得重复。
    /// </summary>
    public List<AbilityLoadout> Loadouts { get; set; } = new List<AbilityLoadout>();
}
