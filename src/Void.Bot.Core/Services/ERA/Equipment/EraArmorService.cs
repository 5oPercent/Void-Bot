using System.Text.Json;
using Void.Bot.Core.Services.ERA.Reward.Models;
using Void.Bot.Message.Enum.ERA;
using System.Net;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public interface IEraArmorService : IScope
{
    Task<GetEraArmorTemplatesResponse> GetTemplatesAsync(GetEraArmorTemplatesRequest request, CancellationToken cancellationToken);
    Task<GetEraArmorTemplateResponse> GetTemplateAsync(GetEraArmorTemplateRequest request, CancellationToken cancellationToken);
    Task<GetEraArmorModificationsResponse> GetModificationsAsync(GetEraArmorModificationsRequest request, CancellationToken cancellationToken);
    Task<GetEraArmorModificationResponse> GetModificationAsync(GetEraArmorModificationRequest request, CancellationToken cancellationToken);
    Task<GetEraArmorSlotConfigurationsResponse> GetSlotConfigurationsAsync(GetEraArmorSlotConfigurationsRequest request, CancellationToken cancellationToken);
    Task<GetEraArmorSlotTypesResponse> GetSlotTypesAsync(GetEraArmorSlotTypesRequest request, CancellationToken cancellationToken);
    Task<GetEraArmorSlotConfigurationResponse> GetSlotConfigurationAsync(GetEraArmorSlotConfigurationRequest request, CancellationToken cancellationToken);
    Task<GetEraArmorSlotTypeResponse> GetSlotTypeAsync(GetEraArmorSlotTypeRequest request, CancellationToken cancellationToken);
    Task<SaveEraArmorTemplateResponse> SaveTemplateAsync(SaveEraArmorTemplateCommand command, CancellationToken cancellationToken);
    Task<SaveEraArmorModificationResponse> SaveModificationAsync(SaveEraArmorModificationCommand command, CancellationToken cancellationToken);
    Task<SaveEraArmorSlotConfigurationResponse> SaveSlotConfigurationAsync(SaveEraArmorSlotConfigurationCommand command, CancellationToken cancellationToken);
    Task<SaveEraArmorSlotTypeResponse> SaveSlotTypeAsync(SaveEraArmorSlotTypeCommand command, CancellationToken cancellationToken);
    Task<DisableEraArmorTemplateResponse> DisableTemplateAsync(DisableEraArmorTemplateCommand command, CancellationToken cancellationToken);
    Task<DeleteEraArmorTemplateResponse> DeleteTemplateAsync(DeleteEraArmorTemplateCommand command, CancellationToken cancellationToken);
    Task<DeleteEraArmorModificationResponse> DeleteModificationAsync(DeleteEraArmorModificationCommand command, CancellationToken cancellationToken);
    Task<DeleteEraArmorSlotConfigurationResponse> DeleteSlotConfigurationAsync(DeleteEraArmorSlotConfigurationCommand command, CancellationToken cancellationToken);
    Task<DeleteEraArmorSlotTypeResponse> DeleteSlotTypeAsync(DeleteEraArmorSlotTypeCommand command, CancellationToken cancellationToken);
    Task<PublishEraArmorConfigurationResponse> PublishConfigurationAsync(PublishEraArmorConfigurationCommand command, CancellationToken cancellationToken);
    Task<ValidateEraArmorConfigurationResponse> ValidateConfigurationAsync(ValidateEraArmorConfigurationRequest request, CancellationToken cancellationToken);
}

/// <summary>Admin 配置用例；HTTP 管理权限由现有 Security 管道校验，配置事务由 Mediator 提交。</summary>
public sealed class EraArmorService : IEraArmorService
{
    private readonly IEraArmorDataProvider _armors;
    private readonly IEraEquipmentDataProvider _equipment;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;

    public EraArmorService(IEraArmorDataProvider armors, IEraEquipmentDataProvider equipment,
        ICurrentUser currentUser, ILocalizationService localization, TimeProvider clock)
    {
        _armors = armors; _equipment = equipment; _currentUser = currentUser; _localization = localization; _clock = clock;
    }

    public async Task<GetEraArmorTemplatesResponse> GetTemplatesAsync(GetEraArmorTemplatesRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Armors.Templates.Where(row => string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        return new GetEraArmorTemplatesResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraArmorTemplateResponse> GetTemplateAsync(GetEraArmorTemplateRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraArmorTemplateResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Armors.Templates.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound) };
    }

    public async Task<GetEraArmorModificationsResponse> GetModificationsAsync(GetEraArmorModificationsRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Armors.Modifications.Where(row => (!request.SlotTypeId.HasValue || row.SlotTypeId == request.SlotTypeId) &&
            (string.IsNullOrWhiteSpace(request.Search) || row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))).ToList();
        return new GetEraArmorModificationsResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraArmorModificationResponse> GetModificationAsync(GetEraArmorModificationRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraArmorModificationResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Armors.Modifications.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound) };
    }

    public async Task<GetEraArmorSlotConfigurationsResponse> GetSlotConfigurationsAsync(GetEraArmorSlotConfigurationsRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Armors.SlotConfigurations.Where(row => string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        return new GetEraArmorSlotConfigurationsResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraArmorSlotTypesResponse> GetSlotTypesAsync(GetEraArmorSlotTypesRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Armors.SlotTypes.Where(row => string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        return new GetEraArmorSlotTypesResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraArmorSlotConfigurationResponse> GetSlotConfigurationAsync(GetEraArmorSlotConfigurationRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraArmorSlotConfigurationResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Armors.SlotConfigurations.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound) };
    }

    public async Task<GetEraArmorSlotTypeResponse> GetSlotTypeAsync(GetEraArmorSlotTypeRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraArmorSlotTypeResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Armors.SlotTypes.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound) };
    }

    public Task<SaveEraArmorTemplateResponse> SaveTemplateAsync(SaveEraArmorTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraArmorTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.Templates.RemoveAll(row => row.Id == command.Template.Id);
            catalog.Templates.Add(command.Template);
            return catalog;
        }, cancellationToken, () => command.Template.Id);

    public Task<SaveEraArmorModificationResponse> SaveModificationAsync(SaveEraArmorModificationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraArmorModificationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.Modifications.RemoveAll(row => row.Id == command.Modification.Id);
            catalog.Modifications.Add(command.Modification);
            return catalog;
        }, cancellationToken, () => command.Modification.Id);

    public Task<SaveEraArmorSlotConfigurationResponse> SaveSlotConfigurationAsync(SaveEraArmorSlotConfigurationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraArmorSlotConfigurationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.SlotConfigurations.RemoveAll(row => row.Id == command.SlotConfiguration.Id);
            catalog.SlotConfigurations.Add(command.SlotConfiguration);
            return catalog;
        }, cancellationToken, () => command.SlotConfiguration.Id);

    public Task<SaveEraArmorSlotTypeResponse> SaveSlotTypeAsync(SaveEraArmorSlotTypeCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraArmorSlotTypeResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.SlotTypes.RemoveAll(row => row.Id == command.SlotType.Id);
            catalog.SlotTypes.Add(command.SlotType);
            return catalog;
        }, cancellationToken, () => command.SlotType.Id);

    public Task<DisableEraArmorTemplateResponse> DisableTemplateAsync(DisableEraArmorTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DisableEraArmorTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            var template = catalog.Templates.SingleOrDefault(row => row.Id == command.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound);
            var disabled = JsonSerializer.Deserialize<EraArmorTemplateDto>(JsonSerializer.Serialize(template))!;
            disabled.IsGenerationEnabled = false;
            catalog.Templates[catalog.Templates.IndexOf(template)] = disabled;
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraArmorTemplateResponse> DeleteTemplateAsync(DeleteEraArmorTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraArmorTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (catalog.Templates.RemoveAll(row => row.Id == command.Id) != 1)
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound);
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraArmorModificationResponse> DeleteModificationAsync(DeleteEraArmorModificationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraArmorModificationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (catalog.Modifications.RemoveAll(row => row.Id == command.Id) != 1)
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound);
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraArmorSlotConfigurationResponse> DeleteSlotConfigurationAsync(DeleteEraArmorSlotConfigurationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraArmorSlotConfigurationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (!catalog.SlotConfigurations.Any(row => row.Id == command.Id))
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound);
            if (catalog.Templates.Any(row => row.SlotConfigurationId == command.Id))
                throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
            catalog.SlotConfigurations.RemoveAll(row => row.Id == command.Id);
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraArmorSlotTypeResponse> DeleteSlotTypeAsync(DeleteEraArmorSlotTypeCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraArmorSlotTypeResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (!catalog.SlotTypes.Any(row => row.Id == command.Id))
                throw Error(HttpStatusCode.NotFound, "era.armor_not_found", value => value.NotFound);
            if (catalog.SlotConfigurations.Any(configuration => configuration.Slots.Any(slot => slot.SlotTypeId == command.Id)) ||
                catalog.Modifications.Any(modification => modification.SlotTypeId == command.Id))
                throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
            catalog.SlotTypes.RemoveAll(row => row.Id == command.Id);
            return catalog;
        }, cancellationToken);

    public Task<PublishEraArmorConfigurationResponse> PublishConfigurationAsync(PublishEraArmorConfigurationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<PublishEraArmorConfigurationResponse>(command.ExpectedManifestVersion, _ => command.Configuration, cancellationToken);

    public async Task<ValidateEraArmorConfigurationResponse> ValidateConfigurationAsync(ValidateEraArmorConfigurationRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var result = new EraArmorConfigurationValidationDto { Version = manifest.Version };
        try
        {
            EraArmorConfigurationValidator.Validate(request.Configuration);
            await CheckReferencesAsync(request.Configuration, manifest, cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException) { result.Errors.Add(_localization.GetValue<EraArmorContextTemplate>(value => value.InvalidConfiguration)); }
        catch (BusinessException) { result.Errors.Add(_localization.GetValue<EraArmorContextTemplate>(value => value.ReferenceConflict)); }
        return new ValidateEraArmorConfigurationResponse { Code = HttpStatusCode.OK, Data = result };
    }

    private async Task<T> WriteAsync<T>(long expectedVersion, Func<EraArmorCatalogDto, EraArmorCatalogDto> apply, CancellationToken cancellationToken, Func<Guid?>? savedId = null)
        where T : EraResponse<EraArmorConfigurationResultDto>, new()
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id == Guid.Empty) throw Error(HttpStatusCode.Unauthorized, "era.armor_unauthorized", value => value.Unauthorized);
        var publication = await _armors.LockPublicationAsync(cancellationToken).ConfigureAwait(false) ??
            throw Error(HttpStatusCode.ServiceUnavailable, "era.armor_config_unavailable", value => value.Unavailable);
        if (expectedVersion < 1 || expectedVersion == long.MaxValue || publication.Version != expectedVersion)
            throw Error(HttpStatusCode.Conflict, "era.armor_config_version_conflict", value => value.VersionConflict);
        var manifest = EraEquipmentService.DecodeConfiguration(publication) ??
            throw Error(HttpStatusCode.ServiceUnavailable, "era.armor_config_unavailable", value => value.Unavailable);
        manifest.Armors = await _armors.GetCatalogAsync(cancellationToken).ConfigureAwait(false);
        manifest.Weapons = await _equipment.GetWeaponCatalogAsync(cancellationToken).ConfigureAwait(false);
        var proposed = apply(new EraArmorCatalogDto
        {
            Templates = manifest.Armors.Templates.ToList(), Modifications = manifest.Armors.Modifications.ToList(),
            SlotTypes = manifest.Armors.SlotTypes.ToList(), SlotConfigurations = manifest.Armors.SlotConfigurations.ToList()
        });
        try { EraArmorConfigurationValidator.Validate(proposed); }
        catch (ArgumentException) { throw Error(HttpStatusCode.BadRequest, "era.armor_invalid_configuration", value => value.InvalidConfiguration); }
        await CheckReferencesAsync(proposed, manifest, cancellationToken).ConfigureAwait(false);
        await _armors.SaveCatalogAsync(manifest.Armors, proposed, cancellationToken).ConfigureAwait(false);
        publication.Version++;
        publication.LastModifiedBy = _currentUser.Id;
        publication.UpdatedAt = _clock.GetUtcNow();
        return new T { Code = HttpStatusCode.OK, Data = new EraArmorConfigurationResultDto { Version = publication.Version, Configuration = proposed, SavedId = savedId?.Invoke() } };
    }

    private async Task CheckReferencesAsync(EraArmorCatalogDto proposed, EraEquipmentManifestDto manifest, CancellationToken cancellationToken)
    {
        if (proposed.Templates.Any(template => manifest.Weapons.Templates.Any(other => other.Id == template.Id)))
            throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
        var templates = proposed.Templates.ToDictionary(row => row.Id);
        var types = proposed.SlotTypes.Select(row => row.Id).ToHashSet();
        var modifications = proposed.Modifications.ToDictionary(row => row.Id);
        var oldSlots = manifest.Armors.SlotConfigurations.SelectMany(row => row.Slots).ToDictionary(row => row.Id);
        var currentSlots = proposed.SlotConfigurations.SelectMany(row => row.Slots).ToDictionary(row => row.Id);
        if (currentSlots.Values.Any(slot => oldSlots.TryGetValue(slot.Id, out var old) && old.SlotTypeId != slot.SlotTypeId))
            throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
        var removedTemplates = manifest.Armors.Templates.Select(row => row.Id).Except(templates.Keys).ToHashSet();
        var changes = new EraEquipmentReferenceChanges
        {
            TemplateIds = manifest.Armors.Templates.Where(old => !templates.TryGetValue(old.Id, out var current) ||
                old.CompatibleSlots.Except(current.CompatibleSlots).Any()).Select(row => row.Id).ToHashSet(),
            ModificationIds = manifest.Armors.Modifications.Where(old => !modifications.TryGetValue(old.Id, out var current) ||
                old.SlotTypeId != current.SlotTypeId).Select(row => row.Id).ToHashSet(),
            SlotTypeIds = manifest.Armors.SlotTypes.Select(row => row.Id).Except(types).ToHashSet(),
            SlotIds = currentSlots.Keys.Except(oldSlots.Keys).ToHashSet()
        };
        // 名称、图标、数值和平衡调整不改变引用，不读取玩家资产或待发计划。
        if (!changes.HasChanges) return;
        var items = await _armors.GetReferencedArmorsAsync(changes, cancellationToken).ConfigureAwait(false);
        var instances = await _armors.GetInstancesAsync(items.Select(row => row.Id), cancellationToken).ConfigureAwait(false);
        foreach (var item in items)
        {
            if (!templates.TryGetValue(item.TemplateId, out var template) ||
                item.EquippedSlot.HasValue && !template.CompatibleSlots.Contains(item.EquippedSlot.Value) ||
                !instances.TryGetValue(item.Id, out var instance))
                throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
            CheckInstance(instance);
        }
        if (removedTemplates.Count > 0)
        {
            var rewardPayload = await _armors.GetRewardConfigurationPayloadAsync(cancellationToken).ConfigureAwait(false);
            if (rewardPayload != null)
            {
                EraRewardRules rewards;
                try
                {
                    rewards = JsonSerializer.Deserialize<EraRewardRules>(rewardPayload) ?? throw new ArgumentException("Reward configuration is missing.");
                    EraRewardConfigurationValidator.Validate(rewards);
                }
                catch (Exception exception) when (exception is ArgumentException or JsonException)
                {
                    throw Error(HttpStatusCode.ServiceUnavailable, "era.armor_config_unavailable", value => value.Unavailable);
                }
                if (rewards.Pools.SelectMany(pool => pool.Entries).Any(entry => removedTemplates.Contains(entry.TemplateId)))
                    throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
            }
        }
        // 计划保存在 JSON 中，只有潜在引用失效时才读取，校验其中受影响的装备。
        foreach (var payload in await _armors.GetPendingRewardPlansAsync(cancellationToken).ConfigureAwait(false))
        {
            EraInventoryRewardPlan? plan;
            try { plan = JsonSerializer.Deserialize<EraInventoryRewardPlan>(payload); }
            catch (JsonException) { throw Error(HttpStatusCode.ServiceUnavailable, "era.armor_config_unavailable", value => value.Unavailable); }
            if (plan?.Grants == null || plan.Grants.Any(grant => grant == null || grant.Equipment == null || grant.Equipment.Any(item => item == null)))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.armor_config_unavailable", value => value.Unavailable);
            foreach (var item in plan.Grants.SelectMany(grant => grant.Equipment).Where(item => item.EquipmentType == EraEquipmentType.Armor))
            {
                if (!EraEquipmentItemRules.ValidArmor(item.ArmorModifications))
                    throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
                var affected = changes.TemplateIds.Contains(item.TemplateId) || item.ArmorModifications!.Slots.Any(slot =>
                    changes.SlotTypeIds.Contains(slot.SlotTypeId) || changes.SlotIds.Contains(slot.SlotId) ||
                    slot.Candidates.Any(candidate => changes.ModificationIds.Contains(candidate.ModDefinitionId)));
                if (!affected) continue;
                if (!templates.ContainsKey(item.TemplateId))
                    throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
                CheckInstance(item.ArmorModifications);
            }
        }
        void CheckInstance(EraArmorInstanceDto? instance)
        {
            if (!EraEquipmentItemRules.ValidArmor(instance) || instance!.Slots.Any(slot => !types.Contains(slot.SlotTypeId) ||
                currentSlots.TryGetValue(slot.SlotId, out var current) && current.SlotTypeId != slot.SlotTypeId ||
                slot.Candidates.Any(candidate => !modifications.TryGetValue(candidate.ModDefinitionId, out var modification) ||
                    modification.SlotTypeId != slot.SlotTypeId)))
                throw Error(HttpStatusCode.Conflict, "era.armor_reference_conflict", value => value.ReferenceConflict);
        }
    }

    private async Task<EraEquipmentManifestDto> ReadAsync(CancellationToken cancellationToken) =>
        await EraEquipmentService.ReadConfigurationAsync(_equipment, false, cancellationToken).ConfigureAwait(false) ??
            throw Error(HttpStatusCode.ServiceUnavailable, "era.armor_config_unavailable", value => value.Unavailable);

    private BusinessException Error(HttpStatusCode status, string code, System.Linq.Expressions.Expression<Func<EraArmorContextTemplate, object>> text) =>
        new EraArmorException(status, _localization.GetValue(text), code);

    private static PagedListDto<T> Page<T>(List<T> items, int pageIndex, int pageSize) where T : new()
    {
        return new PagedListDto<T>
        {
            PageIndex = pageIndex, PageSize = pageSize, Total = items.Count,
            TotalPages = (int)Math.Ceiling((double)items.Count / pageSize),
            Items = items.Skip((int)Math.Min((long)(pageIndex - 1) * pageSize, items.Count)).Take(pageSize).ToList(),
            HasPrevPage = pageIndex > 1, HasNextPage = (long)pageIndex * pageSize < items.Count
        };
    }
}
