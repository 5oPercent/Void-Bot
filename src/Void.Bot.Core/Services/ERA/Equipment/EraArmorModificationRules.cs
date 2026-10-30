using SystemEnum = System.Enum;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

/// <summary>原始六维与改装分开；先累计固定值，再累计百分比，逐属性四舍五入，角色最终上限由战斗层处理。</summary>
public static class EraArmorModificationRules
{
    public static EraArmorAttributesDto Evaluate(EraArmorInstance armor, EraArmorCatalogDto catalog) => Evaluate(new EraArmorAttributesDto
    {
        Strength = armor.Strength, Survival = armor.Survival, Mobility = armor.Mobility,
        Focus = armor.Focus, Efficiency = armor.Efficiency, Resistance = armor.Resistance
    }, armor.Modifications, catalog);

    public static EraArmorAttributesDto Evaluate(EraArmorAttributesDto baseline, EraArmorInstanceDto instance, EraArmorCatalogDto catalog)
    {
        if (!EraEquipmentItemRules.ValidArmor(instance)) throw new ArgumentException("Armor modification instance is invalid.");
        var values = new double[] { baseline.Strength, baseline.Survival, baseline.Mobility, baseline.Focus, baseline.Efficiency, baseline.Resistance };
        var flat = new double[6];
        var percent = new double[6];
        var definitions = catalog.Modifications.ToDictionary(row => row.Id);
        foreach (var slot in instance.Slots)
        {
            var selected = slot.Candidates.Single(row => row.CandidateId == slot.SelectedCandidateId);
            if (!definitions.TryGetValue(selected.ModDefinitionId, out var modification) || modification.SlotTypeId != slot.SlotTypeId)
                throw new ArgumentException("Armor candidate definition or slot type is unavailable.");
            foreach (var modifier in modification.Modifiers)
            {
                int index = (int)modifier.Stat - 1;
                if (index < 0 || index >= values.Length || !double.IsFinite(modifier.Value) || !SystemEnum.IsDefined(modifier.Operation))
                    throw new ArgumentException("Armor attribute modifier is invalid.");
                if (modifier.Operation == EraArmorModifierOperation.Flat) flat[index] += modifier.Value;
                else percent[index] += modifier.Value;
            }
        }
        var result = values.Select((value, index) => (int)Math.Clamp(Math.Round((value + flat[index]) * (1 + percent[index]),
            MidpointRounding.AwayFromZero), 0, int.MaxValue)).ToArray();
        return new EraArmorAttributesDto
        {
            Strength = result[0], Survival = result[1], Mobility = result[2],
            Focus = result[3], Efficiency = result[4], Resistance = result[5]
        };
    }
}
