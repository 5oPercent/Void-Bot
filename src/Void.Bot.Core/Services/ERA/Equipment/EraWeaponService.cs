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

public partial interface IEraWeaponService : IScope
{
    Task<GetEraWeaponTemplatesResponse> GetTemplatesAsync(GetEraWeaponTemplatesRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponTemplateResponse> GetTemplateAsync(GetEraWeaponTemplateRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponModificationsResponse> GetModificationsAsync(GetEraWeaponModificationsRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponModificationResponse> GetModificationAsync(GetEraWeaponModificationRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponSlotConfigurationsResponse> GetSlotConfigurationsAsync(GetEraWeaponSlotConfigurationsRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponSlotTypesResponse> GetSlotTypesAsync(GetEraWeaponSlotTypesRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponSlotConfigurationResponse> GetSlotConfigurationAsync(GetEraWeaponSlotConfigurationRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponSlotTypeResponse> GetSlotTypeAsync(GetEraWeaponSlotTypeRequest request, CancellationToken cancellationToken);
    Task<SaveEraWeaponTemplateResponse> SaveTemplateAsync(SaveEraWeaponTemplateCommand command, CancellationToken cancellationToken);
    Task<SaveEraWeaponModificationResponse> SaveModificationAsync(SaveEraWeaponModificationCommand command, CancellationToken cancellationToken);
    Task<SaveEraWeaponSlotConfigurationResponse> SaveSlotConfigurationAsync(SaveEraWeaponSlotConfigurationCommand command, CancellationToken cancellationToken);
    Task<SaveEraWeaponSlotTypeResponse> SaveSlotTypeAsync(SaveEraWeaponSlotTypeCommand command, CancellationToken cancellationToken);
    Task<DisableEraWeaponTemplateResponse> DisableTemplateAsync(DisableEraWeaponTemplateCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponTemplateResponse> DeleteTemplateAsync(DeleteEraWeaponTemplateCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponModificationResponse> DeleteModificationAsync(DeleteEraWeaponModificationCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponSlotConfigurationResponse> DeleteSlotConfigurationAsync(DeleteEraWeaponSlotConfigurationCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponSlotTypeResponse> DeleteSlotTypeAsync(DeleteEraWeaponSlotTypeCommand command, CancellationToken cancellationToken);
    Task<PublishEraWeaponConfigurationResponse> PublishConfigurationAsync(PublishEraWeaponConfigurationCommand command, CancellationToken cancellationToken);
    Task<ValidateEraWeaponConfigurationResponse> ValidateConfigurationAsync(ValidateEraWeaponConfigurationRequest request, CancellationToken cancellationToken);
}

/// <summary>Admin 配置用例；HTTP 管理权限由现有 Security 管道校验，配置事务由 Mediator 提交。</summary>
public sealed partial class EraWeaponService : IEraWeaponService
{
    private readonly IEraWeaponDataProvider _weapons;
    private readonly IEraEquipmentDataProvider _equipment;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;

    public EraWeaponService(IEraWeaponDataProvider weapons, IEraEquipmentDataProvider equipment,
        ICurrentUser currentUser, ILocalizationService localization, TimeProvider clock)
    {
        _weapons = weapons; _equipment = equipment; _currentUser = currentUser; _localization = localization; _clock = clock;
    }

    public async Task<GetEraWeaponTemplatesResponse> GetTemplatesAsync(GetEraWeaponTemplatesRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Weapons.Templates.Where(row => (!request.FrameworkId.HasValue || row.FrameworkId == request.FrameworkId) &&
            (string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))).ToList();
        return new GetEraWeaponTemplatesResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraWeaponTemplateResponse> GetTemplateAsync(GetEraWeaponTemplateRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraWeaponTemplateResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Weapons.Templates.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound) };
    }

    public async Task<GetEraWeaponModificationsResponse> GetModificationsAsync(GetEraWeaponModificationsRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Weapons.Modifications.Where(row => (!request.SlotTypeId.HasValue || row.SlotTypeId == request.SlotTypeId) &&
            (string.IsNullOrWhiteSpace(request.Search) || row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))).ToList();
        return new GetEraWeaponModificationsResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraWeaponModificationResponse> GetModificationAsync(GetEraWeaponModificationRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraWeaponModificationResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Weapons.Modifications.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound) };
    }

    public async Task<GetEraWeaponSlotConfigurationsResponse> GetSlotConfigurationsAsync(GetEraWeaponSlotConfigurationsRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Weapons.SlotConfigurations.Where(row => string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        return new GetEraWeaponSlotConfigurationsResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraWeaponSlotTypesResponse> GetSlotTypesAsync(GetEraWeaponSlotTypesRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Weapons.SlotTypes.Where(row => string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        return new GetEraWeaponSlotTypesResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraWeaponSlotConfigurationResponse> GetSlotConfigurationAsync(GetEraWeaponSlotConfigurationRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraWeaponSlotConfigurationResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Weapons.SlotConfigurations.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound) };
    }

    public async Task<GetEraWeaponSlotTypeResponse> GetSlotTypeAsync(GetEraWeaponSlotTypeRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraWeaponSlotTypeResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Weapons.SlotTypes.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound) };
    }

    public Task<SaveEraWeaponTemplateResponse> SaveTemplateAsync(SaveEraWeaponTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.Templates.RemoveAll(row => row.Id == command.Template.Id);
            catalog.Templates.Add(command.Template);
            return catalog;
        }, cancellationToken, () => command.Template.Id);

    public Task<SaveEraWeaponModificationResponse> SaveModificationAsync(SaveEraWeaponModificationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponModificationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.Modifications.RemoveAll(row => row.Id == command.Modification.Id);
            catalog.Modifications.Add(command.Modification);
            return catalog;
        }, cancellationToken, () => command.Modification.Id);

    public Task<SaveEraWeaponSlotConfigurationResponse> SaveSlotConfigurationAsync(SaveEraWeaponSlotConfigurationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponSlotConfigurationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.SlotConfigurations.RemoveAll(row => row.Id == command.SlotConfiguration.Id);
            catalog.SlotConfigurations.Add(command.SlotConfiguration);
            return catalog;
        }, cancellationToken, () => command.SlotConfiguration.Id);

    public Task<SaveEraWeaponSlotTypeResponse> SaveSlotTypeAsync(SaveEraWeaponSlotTypeCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponSlotTypeResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.SlotTypes.RemoveAll(row => row.Id == command.SlotType.Id);
            catalog.SlotTypes.Add(command.SlotType);
            return catalog;
        }, cancellationToken, () => command.SlotType.Id);

    public Task<DisableEraWeaponTemplateResponse> DisableTemplateAsync(DisableEraWeaponTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DisableEraWeaponTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            var template = catalog.Templates.SingleOrDefault(row => row.Id == command.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            var disabled = JsonSerializer.Deserialize<EraWeaponTemplateDto>(JsonSerializer.Serialize(template))!;
            disabled.IsGenerationEnabled = false;
            catalog.Templates[catalog.Templates.IndexOf(template)] = disabled;
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraWeaponTemplateResponse> DeleteTemplateAsync(DeleteEraWeaponTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraWeaponTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (catalog.Templates.RemoveAll(row => row.Id == command.Id) != 1)
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraWeaponModificationResponse> DeleteModificationAsync(DeleteEraWeaponModificationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraWeaponModificationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (catalog.Modifications.RemoveAll(row => row.Id == command.Id) != 1)
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraWeaponSlotConfigurationResponse> DeleteSlotConfigurationAsync(DeleteEraWeaponSlotConfigurationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraWeaponSlotConfigurationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (!catalog.SlotConfigurations.Any(row => row.Id == command.Id))
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            if (catalog.Templates.Any(row => row.SlotConfigurationId == command.Id))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            catalog.SlotConfigurations.RemoveAll(row => row.Id == command.Id);
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraWeaponSlotTypeResponse> DeleteSlotTypeAsync(DeleteEraWeaponSlotTypeCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraWeaponSlotTypeResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (!catalog.SlotTypes.Any(row => row.Id == command.Id))
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            if (catalog.SlotConfigurations.Any(configuration => configuration.Slots.Any(slot => slot.SlotTypeId == command.Id)) ||
                catalog.Modifications.Any(modification => modification.SlotTypeId == command.Id))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            catalog.SlotTypes.RemoveAll(row => row.Id == command.Id);
            return catalog;
        }, cancellationToken);

    public Task<PublishEraWeaponConfigurationResponse> PublishConfigurationAsync(PublishEraWeaponConfigurationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<PublishEraWeaponConfigurationResponse>(command.ExpectedManifestVersion, _ => command.Configuration, cancellationToken);

    public async Task<ValidateEraWeaponConfigurationResponse> ValidateConfigurationAsync(ValidateEraWeaponConfigurationRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var result = new EraWeaponConfigurationValidationDto { Version = manifest.Version };
        try
        {
            EraWeaponConfigurationValidator.Validate(request.Configuration);
            await CheckReferencesAsync(request.Configuration, manifest, cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException) { result.Errors.Add(_localization.GetValue<EraWeaponContextTemplate>(value => value.InvalidConfiguration)); }
        catch (BusinessException) { result.Errors.Add(_localization.GetValue<EraWeaponContextTemplate>(value => value.ReferenceConflict)); }
        return new ValidateEraWeaponConfigurationResponse { Code = HttpStatusCode.OK, Data = result };
    }

    private async Task<T> WriteAsync<T>(long expectedVersion, Func<EraWeaponCatalogDto, EraWeaponCatalogDto> apply, CancellationToken cancellationToken, Func<Guid?>? savedId = null)
        where T : EraResponse<EraWeaponConfigurationResultDto>, new()
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id == Guid.Empty) throw Error(HttpStatusCode.Unauthorized, "era.weapon_unauthorized", value => value.Unauthorized);
        var publication = await _weapons.LockPublicationAsync(cancellationToken).ConfigureAwait(false) ??
            throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable);
        if (expectedVersion < 1 || expectedVersion == long.MaxValue || publication.Version != expectedVersion)
            throw Error(HttpStatusCode.Conflict, "era.weapon_config_version_conflict", value => value.VersionConflict);
        var manifest = EraEquipmentService.DecodeConfiguration(publication) ??
            throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable);
        manifest.Weapons = await _weapons.GetCatalogAsync(cancellationToken).ConfigureAwait(false);
        manifest.Armors = await _equipment.GetArmorCatalogAsync(cancellationToken).ConfigureAwait(false);
        var proposed = apply(new EraWeaponCatalogDto
        {
            Frameworks = manifest.Weapons.Frameworks.ToList(), Templates = manifest.Weapons.Templates.ToList(), Modifications = manifest.Weapons.Modifications.ToList(),
            SlotTypes = manifest.Weapons.SlotTypes.ToList(), SlotConfigurations = manifest.Weapons.SlotConfigurations.ToList()
        });
        try { EraWeaponConfigurationValidator.Validate(proposed); }
        catch (ArgumentException) { throw Error(HttpStatusCode.BadRequest, "era.weapon_invalid_configuration", value => value.InvalidConfiguration); }
        await CheckReferencesAsync(proposed, manifest, cancellationToken).ConfigureAwait(false);
        await _weapons.SaveCatalogAsync(manifest.Weapons, proposed, cancellationToken).ConfigureAwait(false);
        publication.Version++;
        publication.LastModifiedBy = _currentUser.Id;
        publication.UpdatedAt = _clock.GetUtcNow();
        return new T { Code = HttpStatusCode.OK, Data = new EraWeaponConfigurationResultDto { Version = publication.Version, Configuration = proposed, SavedId = savedId?.Invoke() } };
    }

    private async Task CheckReferencesAsync(EraWeaponCatalogDto proposed, EraEquipmentManifestDto manifest, CancellationToken cancellationToken)
    {
        if (proposed.Templates.Any(template => manifest.Armors.Templates.Any(other => other.Id == template.Id)))
            throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
        var frameworks = proposed.Frameworks.ToDictionary(row => row.Id);
        var oldFrameworks = manifest.Weapons.Frameworks.ToDictionary(row => row.Id);
        if (proposed.Templates.Any(template => !frameworks[template.FrameworkId].IsEnabled &&
            !manifest.Weapons.Templates.Any(old => old.Id == template.Id && old.FrameworkId == template.FrameworkId)))
            throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
        var templates = proposed.Templates.ToDictionary(row => row.Id);
        var types = proposed.SlotTypes.Select(row => row.Id).ToHashSet();
        var modifications = proposed.Modifications.ToDictionary(row => row.Id);
        var oldSlots = manifest.Weapons.SlotConfigurations.SelectMany(row => row.Slots).ToDictionary(row => row.Id);
        var currentSlots = proposed.SlotConfigurations.SelectMany(row => row.Slots).ToDictionary(row => row.Id);
        if (currentSlots.Values.Any(slot => oldSlots.TryGetValue(slot.Id, out var old) && old.SlotTypeId != slot.SlotTypeId))
            throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
        var removedTemplates = manifest.Weapons.Templates.Select(row => row.Id).Except(templates.Keys).ToHashSet();
        var changes = new EraEquipmentReferenceChanges
        {
            TemplateIds = manifest.Weapons.Templates.Where(old => !templates.TryGetValue(old.Id, out var current) ||
                old.CompatibleSlots.Except(current.CompatibleSlots).Any() ||
                System.Enum.GetValues<EraWeaponStat>().Any(stat => EraWeaponConfigurationValidator.SupportsStat(old.BaseStats, stat) &&
                    !EraWeaponConfigurationValidator.SupportsStat(current.BaseStats, stat)) || old.FrameworkId != current.FrameworkId ||
                oldFrameworks[old.FrameworkId].PresentationDriverKey != frameworks[current.FrameworkId].PresentationDriverKey ||
                !oldFrameworks[old.FrameworkId].Capabilities.Select(capability => capability.Kind).ToHashSet()
                    .SetEquals(frameworks[current.FrameworkId].Capabilities.Select(capability => capability.Kind))).Select(row => row.Id).ToHashSet(),
            ModificationIds = manifest.Weapons.Modifications.Where(old => !modifications.TryGetValue(old.Id, out var current) ||
                old.SlotTypeId != current.SlotTypeId ||
                current.Modifiers.Any(modifier => !old.Modifiers.Any(previous => previous.Stat == modifier.Stat))).Select(row => row.Id).ToHashSet(),
            SlotTypeIds = manifest.Weapons.SlotTypes.Select(row => row.Id).Except(types).ToHashSet(),
            SlotIds = currentSlots.Keys.Except(oldSlots.Keys).ToHashSet()
        };
        // 名称、图标、数值和平衡调整不改变引用，不读取玩家资产或待发计划。
        if (!changes.HasChanges) return;
        var items = await _weapons.GetReferencedWeaponsAsync(changes, cancellationToken).ConfigureAwait(false);
        var instances = await _weapons.GetInstancesAsync(items.Select(row => row.Id), cancellationToken).ConfigureAwait(false);
        foreach (var item in items)
        {
            if (!templates.TryGetValue(item.TemplateId, out var template) ||
                item.EquippedSlot.HasValue && !template.CompatibleSlots.Contains(item.EquippedSlot.Value) ||
                !instances.TryGetValue(item.Id, out var instance))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            CheckInstance(template, instance);
        }
        if (removedTemplates.Count > 0)
        {
            var rewardPayload = await _weapons.GetRewardConfigurationPayloadAsync(cancellationToken).ConfigureAwait(false);
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
                    throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable);
                }
                if (rewards.Pools.SelectMany(pool => pool.Entries).Any(entry => removedTemplates.Contains(entry.TemplateId)))
                    throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            }
        }
        // 计划保存在 JSON 中，只有潜在引用失效时才读取，校验其中受影响的装备。
        foreach (var payload in await _weapons.GetPendingRewardPlansAsync(cancellationToken).ConfigureAwait(false))
        {
            EraInventoryRewardPlan? plan;
            try { plan = JsonSerializer.Deserialize<EraInventoryRewardPlan>(payload); }
            catch (JsonException) { throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable); }
            if (plan?.Grants == null || plan.Grants.Any(grant => grant == null || grant.Equipment == null || grant.Equipment.Any(item => item == null)))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable);
            foreach (var item in plan.Grants.SelectMany(grant => grant.Equipment).Where(item => item.EquipmentType == EraEquipmentType.Weapon))
            {
                if (!EraEquipmentItemRules.ValidWeapon(item.Weapon))
                    throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
                var affected = changes.TemplateIds.Contains(item.TemplateId) || item.Weapon!.Slots.Any(slot =>
                    changes.SlotTypeIds.Contains(slot.SlotTypeId) || changes.SlotIds.Contains(slot.SlotId) ||
                    slot.Candidates.Any(candidate => changes.ModificationIds.Contains(candidate.ModDefinitionId)));
                if (!affected) continue;
                if (!templates.TryGetValue(item.TemplateId, out var template))
                    throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
                CheckInstance(template, item.Weapon);
            }
        }
        void CheckInstance(EraWeaponTemplateDto template, EraWeaponInstanceDto? instance)
        {
            if (!EraEquipmentItemRules.ValidWeapon(instance) || instance!.Slots.Any(slot => !types.Contains(slot.SlotTypeId) ||
                currentSlots.TryGetValue(slot.SlotId, out var current) && current.SlotTypeId != slot.SlotTypeId ||
                slot.Candidates.Any(candidate => !modifications.TryGetValue(candidate.ModDefinitionId, out var modification) ||
                    modification.SlotTypeId != slot.SlotTypeId ||
                        modification.Modifiers.Any(modifier =>
                            !EraWeaponConfigurationValidator.SupportsStat(template.BaseStats, modifier.Stat)))))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
        }
    }

    private async Task<EraEquipmentManifestDto> ReadAsync(CancellationToken cancellationToken) =>
        await EraEquipmentService.ReadConfigurationAsync(_equipment, false, cancellationToken).ConfigureAwait(false) ??
            throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable);

    private BusinessException Error(HttpStatusCode status, string code, System.Linq.Expressions.Expression<Func<EraWeaponContextTemplate, object>> text) =>
        new EraWeaponException(status, _localization.GetValue(text), code);

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
