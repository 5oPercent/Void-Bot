using System.Security.Cryptography;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public interface IEraWeaponGenerator : IScope
{
    EraWeaponInstanceDto Generate(EraWeaponSlotConfigurationDto configuration);
}

/// <summary>只生成固定候选与当前选择，不产生基础属性副本；正式资产与结果由调用事务保存。</summary>
public sealed class EraWeaponGenerator : IEraWeaponGenerator
{
    public EraWeaponInstanceDto Generate(EraWeaponSlotConfigurationDto configuration) => Generate(configuration, RandomNumberGenerator.GetInt32);

    internal static EraWeaponInstanceDto Generate(EraWeaponSlotConfigurationDto configuration, Func<int, int> next)
    {
        var result = new EraWeaponInstanceDto();
        foreach (var slot in configuration.Slots.OrderBy(value => value.DisplayOrder).ThenBy(value => value.Id))
        {
            if (slot.CandidateCount < 1 || slot.CandidateCount > EraWeaponConstants.MaximumCandidatesPerSlot ||
                slot.CandidateCount > slot.Pool.Count || slot.Pool.Any(value => value.Weight <= 0) ||
                slot.Pool.Select(value => value.ModDefinitionId).Distinct().Count() != slot.Pool.Count)
                throw new ArgumentException("Weapon candidate pool cannot satisfy CandidateCount.");
            var available = slot.Pool.ToList();
            var generated = new EraWeaponInstanceSlotDto { SlotId = slot.Id, Name = slot.Name, SlotTypeId = slot.SlotTypeId, DisplayOrder = slot.DisplayOrder };
            for (var index = 0; index < slot.CandidateCount; index++)
            {
                long total = available.Sum(value => (long)value.Weight);
                if (total > int.MaxValue) throw new ArgumentException("Weapon pool total weight exceeds its limit.");
                var roll = next((int)total);
                if (roll < 0 || roll >= total) throw new ArgumentOutOfRangeException(nameof(next));
                var selected = 0;
                while (roll >= available[selected].Weight) roll -= available[selected++].Weight;
                generated.Candidates.Add(new EraWeaponCandidateDto
                {
                    CandidateId = Guid.NewGuid(), ModDefinitionId = available[selected].ModDefinitionId, DisplayOrder = index
                });
                available.RemoveAt(selected);
            }
            generated.SelectedCandidateId = generated.Candidates[0].CandidateId;
            result.Slots.Add(generated);
        }
        return result;
    }
}
