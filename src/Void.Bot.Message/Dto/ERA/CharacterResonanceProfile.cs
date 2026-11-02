namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 角色长期谐振技能配装档案，保存当前晶格体系与各晶格体系配装，不保存局内冷却或载体状态。
/// </summary>
public sealed class CharacterResonanceProfile
{
    /// <summary>
    /// 整份配置或配装的版本，由后端发布／保存时递增。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 角色当前选择的晶格体系 ID；null 表示尚未选择。
    /// </summary>
    public string? SelectedSystemId { get; set; }

    /// <summary>
    /// 每个晶格体系最后保存的四槽配装，晶格体系 ID 不得重复。
    /// </summary>
    public List<AbilityLoadout> Loadouts { get; set; } = new List<AbilityLoadout>();
}
