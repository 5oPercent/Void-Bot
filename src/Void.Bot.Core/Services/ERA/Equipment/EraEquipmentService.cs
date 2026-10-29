using SystemEnum = System.Enum;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

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
    private readonly IEraEquipmentDataProvider _dataProvider;
    private readonly IEraCharacterContext _characterContext;
    private readonly ILocalizationService _localizationService;
    private readonly IEraWeaponDataProvider? _weaponData;
    private readonly IEraArmorDataProvider? _armorData;
    private readonly IEraEquipmentNotificationService? _notifications;

    public EraEquipmentService(
        IEraEquipmentDataProvider dataProvider,
        IEraCharacterContext characterContext,
        ILocalizationService localizationService,
        IEraWeaponDataProvider? weaponData = null,
        IEraEquipmentNotificationService? notifications = null, IEraArmorDataProvider? armorData = null)
    {
        _dataProvider = dataProvider;
        _characterContext = characterContext;
        _localizationService = localizationService;
        _weaponData = weaponData;
        _armorData = armorData;
        _notifications = notifications;
    }

    public async Task<GetEraEquipmentManifestResponse> GetManifestAsync(
        GetEraEquipmentManifestRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadConfigurationAsync(_dataProvider, false, cancellationToken).ConfigureAwait(false);
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
        return await GetStateAsync<GetEraCharacterInventoryResponse>(request.CharacterId, request.Slot, cancellationToken, request.EquipmentType)
            .ConfigureAwait(false);
    }

    private async Task<T> GetStateAsync<T>(Guid characterId, EraEquipmentSlot? slot, CancellationToken cancellationToken,
        EraEquipmentType? equipmentType = null)
        where T : EraResponse<EraCharacterEquipmentStateDto>, new()
    {
        var character = await _characterContext.RequireOwnedAsync(characterId, false, cancellationToken)
            .ConfigureAwait(false);
        if (slot.HasValue && !EraEquipmentRules.EquippedSlots.Contains(slot.Value))
        {
            return Error<T>(HttpStatusCode.BadRequest, "era.equipment_invalid_slot", text => text.InvalidSlot);
        }
        if (equipmentType.HasValue && !SystemEnum.IsDefined(equipmentType.Value))
            return Error<T>(HttpStatusCode.BadRequest, "era.equipment_invalid_type", text => text.InvalidEquipmentType);

        EraEquipmentReadSnapshot? snapshot;
        try
        {
            snapshot = await _dataProvider.GetCharacterSnapshotAsync(character.Id, character.PlayerId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (EraEquipmentConcurrencyException)
        {
            return Error<T>(HttpStatusCode.Conflict, "era.equipment_version_conflict", text => text.VersionConflict);
        }
        if (snapshot == null || snapshot.CharacterId != character.Id || snapshot.PlayerId != character.PlayerId)
        {
            throw new EraPlayerAccessException(HttpStatusCode.NotFound, "era.character_not_found",
                _localizationService.GetValue<EraContextTemplate>(text => text.CharacterNotFound));
        }

        var manifest = await ReadConfigurationAsync(_dataProvider, false, cancellationToken).ConfigureAwait(false);
        var state = CreateState(snapshot, manifest, slot, equipmentType);
        if (state == null)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        }

        return new T { Code = HttpStatusCode.OK, Data = state };
    }

    internal static EraCharacterEquipmentStateDto? CreateState(
        EraEquipmentReadSnapshot snapshot, EraEquipmentManifestDto? manifest, EraEquipmentSlot? slot,
        EraEquipmentType? equipmentType = null)
    {
        var slots = EraEquipmentRules.EquippedSlots;
        if (snapshot.EquipmentVersion < 0 || snapshot.Entries.Select(entry => entry.Equipment.Id).Distinct().Count() != snapshot.Entries.Count)
        {
            return null;
        }

        var definitions = manifest?.Armors.Templates.ToDictionary(definition => definition.Id);
        var items = new List<EraEquipmentInstanceDto>();
        foreach (var entry in snapshot.Entries)
        {
            var equipment = entry.Equipment;
            var armor = entry.Armor;
            if (equipment.Id == Guid.Empty || equipment.CharacterId != snapshot.CharacterId || equipment.OwnerPlayerId != snapshot.PlayerId ||
                !EraEquipmentItemRules.ValidPayload(entry) || !SystemEnum.IsDefined(equipment.Grade) ||
                equipment.Version < 1 || equipment.DefinitionVersion < 1 ||
                equipment.DefinitionId == Guid.Empty || equipment.CreatedAt == default ||
                equipment.Location is not (EraEquipmentLocation.Inventory or EraEquipmentLocation.Equipped) ||
                (equipment.Location == EraEquipmentLocation.Equipped) != equipment.EquippedSlot.HasValue ||
                (equipment.EquippedSlot.HasValue && !slots.Contains(equipment.EquippedSlot.Value)))
            {
                return null;
            }

            EraArmorTemplateDto? definition = null;
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
                EquippedSlot = equipment.EquippedSlot,
                Version = equipment.Version,
                CreatedAt = equipment.CreatedAt,
                Share = entry.Share,
                Weapon = entry.Weapon,
                ArmorModifications = armor?.Modifications,
                ArmorEffectiveAttributes = armor == null || manifest == null ? null : EraArmorModificationRules.Evaluate(armor, manifest.Armors),
                WeaponTemplate = manifest?.Weapons.Templates.SingleOrDefault(template => template.Id == equipment.DefinitionId),
                GenerationRuleVersion = armor?.GenerationRuleVersion ?? 0,
                ArmorAttributes = armor == null ? new EraArmorAttributesDto() : new EraArmorAttributesDto
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
        if (inventory.Count > EraEquipmentRules.InventoryCapacity ||
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
            FilterEquipmentType = equipmentType,
            TotalInventoryCapacity = EraEquipmentRules.InventoryCapacity,
            TotalInventoryCount = inventory.Count,
            InventoryItems = inventory.Where(item => !equipmentType.HasValue || item.EquipmentType == equipmentType.Value)
                .Where(item => !slot.HasValue || (item.EquipmentType == EraEquipmentType.Armor
                    ? item.Definition?.CompatibleSlots.Contains(slot.Value) == true
                    : item.WeaponTemplate?.CompatibleSlots.Contains(slot.Value) == true)).ToList(),
            EquippedSlots = slots.Select(equippedSlot => new EraEquippedArmorSlotDto
            {
                Slot = equippedSlot,
                Equipment = equipped.SingleOrDefault(item => item.EquippedSlot == equippedSlot)
            }).ToList()
        };
    }

    internal static EraEquipmentManifestDto? DecodeConfiguration(EraEquipmentConfiguration? record)
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
                GradeBudgets = SystemEnum.GetValues<EraEquipmentGrade>().Select(grade => new EraArmorGradeBudgetDto
                {
                    Grade = grade,
                    Budget = EraArmorGenerationRules.GetBudget(grade)
                }).ToList()
            };
            manifest.InventoryCapacity = EraEquipmentRules.InventoryCapacity;
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

    internal static async Task<EraEquipmentManifestDto?> ReadConfigurationAsync(
        IEraEquipmentDataProvider provider, bool locked, CancellationToken cancellationToken)
    {
        // 发布版本包围关系表读取，普通 GET 不开写事务；写命令已持有目录共享锁。
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var row = locked ? await provider.LockConfigurationAsync(cancellationToken).ConfigureAwait(false) :
                await provider.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            var result = DecodeConfiguration(row);
            if (result == null) return null;
            try
            {
                result.Weapons = await provider.GetWeaponCatalogAsync(cancellationToken).ConfigureAwait(false) ?? new EraWeaponCatalogDto();
                EraWeaponConfigurationValidator.Validate(result.Weapons);
                result.Armors = await provider.GetArmorCatalogAsync(cancellationToken).ConfigureAwait(false) ?? new EraArmorCatalogDto();
                EraArmorConfigurationValidator.Validate(result.Armors);
            }
            catch (Exception exception) when (exception is ArgumentException or JsonException) { return null; }
            if (result.Armors.Templates.Select(definition => definition.Id).Intersect(result.Weapons.Templates.Select(template => template.Id)).Any())
                return null;
            if (locked) return result;
            var after = await provider.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            if (after?.Version == row!.Version) return result;
        }
        return null;
    }

    private T Error<T>(HttpStatusCode status, string key, Expression<Func<EraEquipmentContextTemplate, object>> text)
        where T : VoidBotResponse, new()
    {
        return new T { Code = status, ErrorCode = key, Message = _localizationService.GetValue(text) };
    }
}
