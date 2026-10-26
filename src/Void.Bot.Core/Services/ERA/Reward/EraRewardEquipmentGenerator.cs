using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Reward.Models;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Reward;

/// <summary>将已确定的奖励条目生成装备快照；不抽池、不写库、不决定发放位置。</summary>
public static class EraRewardEquipmentGenerator
{
    public static List<EraPlannedRewardEquipment> Generate(IEnumerable<EraEquipmentRewardEntry> entries,
        EraEquipmentManifestDto manifest, IEraArmorGenerator armor, IEraWeaponGenerator? weapons)
    {
        var result = new List<EraPlannedRewardEquipment>();
        var validator = new EraEquipmentRewardEntryValidator();
        var definitions = manifest.Armors.Templates.ToDictionary(item => item.Id);
        var templates = manifest.Weapons.Templates.ToDictionary(item => item.Id);
        var configurations = manifest.Weapons.SlotConfigurations.ToDictionary(item => item.Id);
        foreach (var entry in entries)
        {
            if (entry == null || !validator.Validate(entry).IsValid || (long)result.Count + entry.Count > EraEquipmentRules.InventoryCapacity)
                throw new ArgumentException("Reward entries are invalid.");
            var isWeapon = templates.TryGetValue(entry.DefinitionId, out var template);
            if (isWeapon ? !template!.IsGenerationEnabled || weapons == null : !definitions.TryGetValue(entry.DefinitionId, out var availableArmor) || !availableArmor.IsGenerationEnabled)
                throw new ArgumentException("Reward template is unavailable.");
            for (var index = 0; index < entry.Count; index++)
            {
                var item = new EraPlannedRewardEquipment
                {
                    EquipmentId = Guid.NewGuid(), DefinitionId = entry.DefinitionId, Grade = entry.Grade,
                    EquipmentType = isWeapon ? EraEquipmentType.Weapon : EraEquipmentType.Armor
                };
                if (isWeapon) item.Weapon = template!.SlotConfigurationId is { } configurationId
                    ? weapons!.Generate(configurations[configurationId]) : new EraWeaponInstanceDto();
                else
                {
                    var generated = armor.Generate(entry.Grade);
                    var armorTemplate = definitions[entry.DefinitionId];
                    if (armorTemplate.SlotConfigurationId.HasValue)
                        item.ArmorModifications = new EraArmorModificationGenerator().Generate(manifest.Armors.SlotConfigurations.Single(row => row.Id == armorTemplate.SlotConfigurationId.Value));
                    item.GenerationRuleVersion = generated.GenerationRuleVersion;
                    item.Attributes = generated.Attributes;
                }
                result.Add(item);
            }
        }
        return result;
    }
}
