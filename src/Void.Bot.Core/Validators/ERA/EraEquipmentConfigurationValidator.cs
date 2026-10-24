using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>验证当前展示定义，不用当前生成规则重新校验历史装备属性。</summary>
public static class EraEquipmentConfigurationValidator
{
    public static void Validate(EraEquipmentManifestDto configuration)
    {
        if (configuration == null || configuration.SchemaVersion != 1 || configuration.Version < 1 ||
            configuration.Definitions == null || configuration.Definitions.Count > 10000 || configuration.Definitions.Any(definition => definition == null))
        {
            throw new ArgumentException("Equipment configuration schema or version is invalid.");
        }

        var ids = new HashSet<Guid>();
        foreach (var definition in configuration.Definitions)
        {
            if (definition.Id == Guid.Empty || !ids.Add(definition.Id) ||
                string.IsNullOrWhiteSpace(definition.Name) || definition.Name.Length > 128 ||
                definition.Icon is not { Length: <= 256 } || definition.Note is not { Length: <= 2048 } ||
                definition.CompatibleSlots == null || definition.CompatibleSlots.Count == 0 ||
                definition.CompatibleSlots.Any(slot => !Void.Bot.Core.Services.ERA.Equipment.EraEquipmentRules.ArmorSlots.Contains(slot)) ||
                definition.CompatibleSlots.Distinct().Count() != definition.CompatibleSlots.Count)
            {
                throw new ArgumentException("Equipment definition or slot mapping is invalid.");
            }
        }
    }
}
