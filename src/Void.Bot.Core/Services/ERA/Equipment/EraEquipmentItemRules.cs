using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public static class EraEquipmentItemRules
{
    public static bool ValidWeapon(EraWeaponInstanceDto? weapon)
    {
        return weapon != null && weapon.Slots != null &&
            weapon.Slots.All(slot => slot != null && slot.SlotId != Guid.Empty && slot.SlotTypeId != Guid.Empty &&
                !string.IsNullOrWhiteSpace(slot.Name) && slot.Name.Length <= 128 && slot.DisplayOrder >= 0 && slot.Candidates != null &&
                slot.Candidates.Count is >= 1 and <= EraWeaponConstants.MaximumCandidatesPerSlot &&
                slot.Candidates.All(candidate => candidate != null && candidate.CandidateId != Guid.Empty &&
                    candidate.ModDefinitionId != Guid.Empty && candidate.DisplayOrder >= 0) &&
                slot.Candidates.Select(candidate => candidate.CandidateId).Distinct().Count() == slot.Candidates.Count &&
                slot.Candidates.Select(candidate => candidate.ModDefinitionId).Distinct().Count() == slot.Candidates.Count &&
                slot.Candidates.Any(candidate => candidate.CandidateId == slot.SelectedCandidateId)) &&
            weapon.Slots.Select(slot => slot.SlotId).Distinct().Count() == weapon.Slots.Count &&
            weapon.Slots.SelectMany(slot => slot.Candidates).Select(candidate => candidate.CandidateId).Distinct().Count() ==
                weapon.Slots.Sum(slot => slot.Candidates.Count);
    }

    public static bool ValidArmor(EraArmorInstanceDto? weapon)
    {
        return weapon != null && weapon.Slots != null &&
            weapon.Slots.All(slot => slot != null && slot.SlotId != Guid.Empty && slot.SlotTypeId != Guid.Empty &&
                !string.IsNullOrWhiteSpace(slot.Name) && slot.Name.Length <= 128 && slot.DisplayOrder >= 0 && slot.Candidates != null &&
                slot.Candidates.Count is >= 1 and <= EraArmorConstants.MaximumCandidatesPerSlot &&
                slot.Candidates.All(candidate => candidate != null && candidate.CandidateId != Guid.Empty &&
                    candidate.ModDefinitionId != Guid.Empty && candidate.DisplayOrder >= 0) &&
                slot.Candidates.Select(candidate => candidate.CandidateId).Distinct().Count() == slot.Candidates.Count &&
                slot.Candidates.Select(candidate => candidate.ModDefinitionId).Distinct().Count() == slot.Candidates.Count &&
                slot.Candidates.Any(candidate => candidate.CandidateId == slot.SelectedCandidateId)) &&
            weapon.Slots.Select(slot => slot.SlotId).Distinct().Count() == weapon.Slots.Count &&
            weapon.Slots.SelectMany(slot => slot.Candidates).Select(candidate => candidate.CandidateId).Distinct().Count() ==
                weapon.Slots.Sum(slot => slot.Candidates.Count);
    }

    public static bool ValidPayload(EraEquipmentReadEntry entry)
    {
        var item = entry.Equipment;
        if (item.EquipmentType == EraEquipmentType.Weapon)
            return entry.Armor == null && ValidWeapon(entry.Weapon) &&
                (!item.EquippedSlot.HasValue || item.EquippedSlot.Value >= EraEquipmentSlot.Weapon1 && item.EquippedSlot.Value <= EraEquipmentSlot.Weapon3);
        var armor = entry.Armor;
        return item.EquipmentType == EraEquipmentType.Armor && entry.Weapon == null &&
            (!item.EquippedSlot.HasValue || EraEquipmentRules.ArmorSlots.Contains(item.EquippedSlot.Value)) &&
            armor != null && ValidArmor(armor.Modifications) && armor.EquipmentId == item.Id && armor.GenerationRuleVersion > 0 &&
            armor.Strength >= 0 && armor.Survival >= 0 && armor.Mobility >= 0 && armor.Focus >= 0 && armor.Efficiency >= 0 && armor.Resistance >= 0;
    }
}
