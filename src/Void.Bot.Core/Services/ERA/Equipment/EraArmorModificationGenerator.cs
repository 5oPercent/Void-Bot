using System.Security.Cryptography;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public interface IEraArmorModificationGenerator : IScope
{
    EraArmorInstanceDto Generate(EraArmorSlotConfigurationDto configuration);
}

/// <summary>只生成固定候选与当前选择，不产生基础属性副本；正式资产与结果由调用事务保存。</summary>
public sealed class EraArmorModificationGenerator : IEraArmorModificationGenerator
{
    public EraArmorInstanceDto Generate(EraArmorSlotConfigurationDto configuration) => Generate(configuration, RandomNumberGenerator.GetInt32);

    internal static EraArmorInstanceDto Generate(EraArmorSlotConfigurationDto configuration, Func<int, int> next)
    {
        if (configuration.Slots.Count < 1) throw new ArgumentException("Armor slot configuration must contain at least one slot.");
        var result = new EraArmorInstanceDto();
        foreach (var slot in configuration.Slots.OrderBy(value => value.DisplayOrder).ThenBy(value => value.Id))
        {
            var sampled = EraEquipmentModificationSampler.Sample(slot.Pool.Select(row => (row.ModDefinitionId, row.Weight)),
                slot.CandidateCount, EraArmorConstants.MaximumCandidatesPerSlot, next);
            var generated = new EraArmorInstanceSlotDto
            {
                SlotId = slot.Id, Name = slot.Name, SlotTypeId = slot.SlotTypeId, DisplayOrder = slot.DisplayOrder,
                Candidates = sampled.Select((definitionId, index) => new EraArmorCandidateDto
                {
                    CandidateId = Guid.NewGuid(), ModDefinitionId = definitionId, DisplayOrder = index
                }).ToList()
            };
            generated.SelectedCandidateId = generated.Candidates[0].CandidateId;
            result.Slots.Add(generated);
        }
        return result;
    }
}
