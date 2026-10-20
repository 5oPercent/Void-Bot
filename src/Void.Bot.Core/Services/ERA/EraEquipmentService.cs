using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraEquipmentService : IScope
{
    Task<GetEraEquipmentManifestResponse> GetManifestAsync(
        GetEraEquipmentManifestRequest request, CancellationToken cancellationToken);
    Task<GetEraCharacterEquipmentResponse> GetEquipmentAsync(
        GetEraCharacterEquipmentRequest request, CancellationToken cancellationToken);
    Task<GetEraCharacterInventoryResponse> GetInventoryAsync(
        GetEraCharacterInventoryRequest request, CancellationToken cancellationToken);
}

/// <summary>装备查询与装配业务；不生成资产，写事务由 Mediator 工作单元统一提交。</summary>
public sealed partial class EraEquipmentService : IEraEquipmentService
{
    private const int InventoryCapacityPerSlot = 12;
    private readonly IEraEquipmentDataProvider _dataProvider;
    private readonly IEraCharacterContext _characterContext;
    private readonly ILocalizationService _localizationService;

    public EraEquipmentService(
        IEraEquipmentDataProvider dataProvider,
        IEraCharacterContext characterContext,
        ILocalizationService localizationService)
    {
        _dataProvider = dataProvider;
        _characterContext = characterContext;
        _localizationService = localizationService;
    }

    public async Task<GetEraEquipmentManifestResponse> GetManifestAsync(
        GetEraEquipmentManifestRequest request, CancellationToken cancellationToken)
    {
        var record = await _dataProvider.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var manifest = DecodeConfiguration(record);
        if (manifest == null)
        {
            return Error<GetEraEquipmentManifestResponse>(HttpStatusCode.ServiceUnavailable,
                "era.equipment_config_unavailable", text => text.ConfigUnavailable);
        }

        return new GetEraEquipmentManifestResponse { Code = HttpStatusCode.OK, Data = manifest };
    }

    public async Task<GetEraCharacterEquipmentResponse> GetEquipmentAsync(
        GetEraCharacterEquipmentRequest request, CancellationToken cancellationToken)
    {
        return await GetStateAsync<GetEraCharacterEquipmentResponse>(request.CharacterId, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<GetEraCharacterInventoryResponse> GetInventoryAsync(
        GetEraCharacterInventoryRequest request, CancellationToken cancellationToken)
    {
        return await GetStateAsync<GetEraCharacterInventoryResponse>(request.CharacterId, request.Slot, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<T> GetStateAsync<T>(Guid characterId, EraArmorSlot? slot, CancellationToken cancellationToken)
        where T : EraResponse<EraCharacterEquipmentStateDto>, new()
    {
        var character = await _characterContext.RequireOwnedAsync(characterId, false, cancellationToken)
            .ConfigureAwait(false);
        if (slot.HasValue && !System.Enum.IsDefined(slot.Value))
        {
            return Error<T>(HttpStatusCode.BadRequest, "era.equipment_invalid_slot", text => text.InvalidSlot);
        }

        var snapshot = await _dataProvider.GetCharacterSnapshotAsync(character.Id, character.PlayerId, cancellationToken)
            .ConfigureAwait(false);
        if (snapshot == null || snapshot.CharacterId != character.Id || snapshot.PlayerId != character.PlayerId)
        {
            throw new EraPlayerAccessException(HttpStatusCode.NotFound, "era.character_not_found",
                _localizationService.GetValue<EraContextTemplate>(text => text.CharacterNotFound));
        }

        var configuration = await _dataProvider.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var manifest = DecodeConfiguration(configuration);
        var state = CreateState(snapshot, manifest, slot);
        if (state == null)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        }

        return new T { Code = HttpStatusCode.OK, Data = state };
    }

    private static EraCharacterEquipmentStateDto? CreateState(
        EraEquipmentReadSnapshot snapshot, EraEquipmentManifestDto? manifest, EraArmorSlot? slot)
    {
        var slots = System.Enum.GetValues<EraArmorSlot>();
        if (snapshot.EquipmentVersion < 0 || snapshot.Entries.Select(entry => entry.Equipment.Id).Distinct().Count() != snapshot.Entries.Count)
        {
            return null;
        }

        var definitions = manifest?.Definitions.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
        var items = new List<EraEquipmentInstanceDto>();
        foreach (var entry in snapshot.Entries)
        {
            var equipment = entry.Equipment;
            var armor = entry.Armor;
            if (equipment.Id == Guid.Empty || equipment.CharacterId != snapshot.CharacterId || equipment.OwnerPlayerId != snapshot.PlayerId ||
                equipment.EquipmentType != EraEquipmentType.Armor || !System.Enum.IsDefined(equipment.Grade) ||
                !System.Enum.IsDefined(equipment.InventorySlot) || equipment.Version < 1 || equipment.DefinitionVersion < 1 ||
                string.IsNullOrWhiteSpace(equipment.DefinitionId) || equipment.CreatedAt == default ||
                equipment.Location is not (EraEquipmentLocation.Inventory or EraEquipmentLocation.Equipped) ||
                (equipment.Location == EraEquipmentLocation.Equipped) != equipment.EquippedSlot.HasValue ||
                (equipment.EquippedSlot.HasValue && !System.Enum.IsDefined(equipment.EquippedSlot.Value)) ||
                armor == null || armor.EquipmentId != equipment.Id || armor.GenerationRuleVersion < 1 ||
                armor.Strength < 0 || armor.Survival < 0 || armor.Mobility < 0 ||
                armor.Focus < 0 || armor.Efficiency < 0 || armor.Resistance < 0)
            {
                return null;
            }

            EraArmorDefinitionDto? definition = null;
            definitions?.TryGetValue(equipment.DefinitionId, out definition);
            items.Add(new EraEquipmentInstanceDto
            {
                Id = equipment.Id,
                DefinitionId = equipment.DefinitionId,
                DefinitionVersion = equipment.DefinitionVersion,
                Definition = definition,
                EquipmentType = equipment.EquipmentType,
                Grade = equipment.Grade,
                OwnerPlayerId = snapshot.PlayerId,
                CharacterId = snapshot.CharacterId,
                Location = equipment.Location,
                InventorySlot = equipment.InventorySlot,
                EquippedSlot = equipment.EquippedSlot,
                Version = equipment.Version,
                CreatedAt = equipment.CreatedAt,
                GenerationRuleVersion = armor.GenerationRuleVersion,
                ArmorAttributes = new EraArmorAttributesDto
                {
                    Strength = armor.Strength,
                    Survival = armor.Survival,
                    Mobility = armor.Mobility,
                    Focus = armor.Focus,
                    Efficiency = armor.Efficiency,
                    Resistance = armor.Resistance
                }
            });
        }

        var inventory = items.Where(item => item.Location == EraEquipmentLocation.Inventory)
            .OrderBy(item => item.CreatedAt).ThenBy(item => item.Id).ToList();
        var equipped = items.Where(item => item.Location == EraEquipmentLocation.Equipped).ToList();
        if (slots.Any(partition => inventory.Count(item => item.InventorySlot == partition) > InventoryCapacityPerSlot) ||
            equipped.GroupBy(item => item.EquippedSlot).Any(group => group.Count() > 1))
        {
            return null;
        }

        return new EraCharacterEquipmentStateDto
        {
            CharacterId = snapshot.CharacterId,
            IsCharacterDisabled = snapshot.IsCharacterDisabled,
            EquipmentVersion = snapshot.EquipmentVersion,
            EquippedOperationRestriction = snapshot.EquippedOperationRestriction,
            ManifestVersion = manifest?.Version,
            FilterSlot = slot,
            TotalInventoryCapacity = slots.Length * InventoryCapacityPerSlot,
            TotalInventoryCount = inventory.Count,
            InventoryPartitions = slots.Select(partition => new EraArmorInventoryPartitionDto
            {
                Slot = partition,
                Count = inventory.Count(item => item.InventorySlot == partition),
                Capacity = InventoryCapacityPerSlot
            }).ToList(),
            InventoryItems = slot.HasValue
                ? inventory.Where(item => item.InventorySlot == slot.Value &&
                    (item.Definition == null || item.Definition.CompatibleSlots.Contains(slot.Value))).ToList()
                : inventory,
            EquippedSlots = slots.Select(equippedSlot => new EraEquippedArmorSlotDto
            {
                Slot = equippedSlot,
                Equipment = equipped.SingleOrDefault(item => item.EquippedSlot == equippedSlot)
            }).ToList()
        };
    }

    private static EraEquipmentManifestDto? DecodeConfiguration(EraEquipmentConfiguration? record)
    {
        if (record == null)
        {
            return null;
        }

        try
        {
            var manifest = JsonSerializer.Deserialize<EraEquipmentManifestDto>(record.Payload);
            if (manifest == null)
            {
                return null;
            }

            manifest.Version = record.Version;
            EraEquipmentConfigurationValidator.Validate(manifest);
            manifest.GenerationRules = new EraArmorGenerationRulesDto
            {
                Version = EraArmorGenerationRules.Version,
                WeightsPercent = EraArmorGenerationRules.WeightsPercent.ToList(),
                MaximumDeviation = EraArmorGenerationRules.MaximumDeviation,
                GradeBudgets = System.Enum.GetValues<EraArmorGrade>().Select(grade => new EraArmorGradeBudgetDto
                {
                    Grade = grade,
                    Budget = EraArmorGenerationRules.GetBudget(grade)
                }).ToList()
            };
            manifest.InventoryCapacityPerSlot = InventoryCapacityPerSlot;
            return manifest;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private T Error<T>(HttpStatusCode status, string key, Expression<Func<EraEquipmentContextTemplate, object>> text)
        where T : VoidBotResponse, new()
    {
        return new T { Code = status, ErrorCode = key, Message = _localizationService.GetValue(text) };
    }
}
