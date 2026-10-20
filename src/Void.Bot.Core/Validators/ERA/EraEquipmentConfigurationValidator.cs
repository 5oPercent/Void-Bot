using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

/// <summary>验证当前展示定义，不用当前生成规则重新校验历史装备属性。</summary>
public static class EraEquipmentConfigurationValidator
{
    public static void Validate(EraEquipmentManifestDto configuration)
    {
        if (configuration.SchemaVersion != 1 || configuration.Version < 1 ||
            configuration.Definitions == null || configuration.Definitions.Count > 10000)
        {
            throw new ArgumentException("Equipment configuration schema or version is invalid.");
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var definition in configuration.Definitions)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id) || definition.Id.Length > 128 ||
                definition.Id != definition.Id.Trim() || !ids.Add(definition.Id) ||
                string.IsNullOrWhiteSpace(definition.Name) || definition.Name.Length > 128 ||
                definition.Icon == null || definition.Icon.Length > 256 ||
                definition.Note == null || definition.Note.Length > 2048 ||
                !System.Enum.IsDefined(definition.InventorySlot) ||
                definition.CompatibleSlots == null || definition.CompatibleSlots.Count == 0 ||
                definition.CompatibleSlots.Any(slot => !System.Enum.IsDefined(slot)) ||
                definition.CompatibleSlots.Distinct().Count() != definition.CompatibleSlots.Count ||
                !definition.CompatibleSlots.Contains(definition.InventorySlot))
            {
                throw new ArgumentException("Equipment definition or slot mapping is invalid.");
            }
        }
    }
}
