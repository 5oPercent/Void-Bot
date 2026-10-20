namespace Void.Bot.Core.Services.ERA.Player;

/// <summary>后端必须识别的 ERA 核心配置键。</summary>
public static class EraConfigurationKeys
{
    public const string AttributeMinimum = "player.attribute.minimum";
    public const string AttributeMaximum = "player.attribute.maximum";
    public const string BaseStrength = "player.strength.base";
    public const string BaseConstitution = "player.constitution.base";
    public const string BaseAgility = "player.agility.base";
    public const string BaseHealth = "player.health.base";
    public const string StrengthBonusPercent = "player.strength.full_bonus_percent";
    public const string ConstitutionBonusPercent = "player.constitution.full_bonus_percent";
    public const string AgilityBonusPercent = "player.agility.full_bonus_percent";
    public const string CharacterLimit = "character.active_limit";
    public const string DefaultWarehouseCapacity = "warehouse.default_capacity";
}
