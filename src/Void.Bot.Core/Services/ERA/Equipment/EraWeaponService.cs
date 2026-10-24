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

public interface IEraWeaponService : IScope
{
    Task<GetEraWeaponTemplatesResponse> GetTemplatesAsync(GetEraWeaponTemplatesRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponTemplateResponse> GetTemplateAsync(GetEraWeaponTemplateRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponModificationsResponse> GetModificationsAsync(GetEraWeaponModificationsRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponModificationResponse> GetModificationAsync(GetEraWeaponModificationRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponStatMappingsResponse> GetStatMappingsAsync(GetEraWeaponStatMappingsRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponStatMappingResponse> GetStatMappingAsync(GetEraWeaponStatMappingRequest request, CancellationToken cancellationToken);
    Task<SaveEraWeaponTemplateResponse> SaveTemplateAsync(SaveEraWeaponTemplateCommand command, CancellationToken cancellationToken);
    Task<SaveEraWeaponModificationResponse> SaveModificationAsync(SaveEraWeaponModificationCommand command, CancellationToken cancellationToken);
    Task<SaveEraWeaponStatMappingResponse> SaveStatMappingAsync(SaveEraWeaponStatMappingCommand command, CancellationToken cancellationToken);
    Task<DisableEraWeaponTemplateResponse> DisableTemplateAsync(DisableEraWeaponTemplateCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponTemplateResponse> DeleteTemplateAsync(DeleteEraWeaponTemplateCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponModificationResponse> DeleteModificationAsync(DeleteEraWeaponModificationCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponStatMappingResponse> DeleteStatMappingAsync(DeleteEraWeaponStatMappingCommand command, CancellationToken cancellationToken);
    Task<PublishEraWeaponConfigurationResponse> PublishConfigurationAsync(PublishEraWeaponConfigurationCommand command, CancellationToken cancellationToken);
    Task<ValidateEraWeaponConfigurationResponse> ValidateConfigurationAsync(ValidateEraWeaponConfigurationRequest request, CancellationToken cancellationToken);
}

/// <summary>Admin 配置用例；HTTP 管理权限由现有 Security 管道校验，配置事务由 Mediator 提交。</summary>
public sealed class EraWeaponService : IEraWeaponService
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
        var items = manifest.Weapons.Templates.Where(row => string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
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
        var items = manifest.Weapons.Modifications.Where(row => (!request.Slot.HasValue || row.Slot == request.Slot) &&
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

    public async Task<GetEraWeaponStatMappingsResponse> GetStatMappingsAsync(GetEraWeaponStatMappingsRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Weapons.StatMappings.Where(row => string.IsNullOrWhiteSpace(request.Search) ||
            row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        return new GetEraWeaponStatMappingsResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraWeaponStatMappingResponse> GetStatMappingAsync(GetEraWeaponStatMappingRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraWeaponStatMappingResponse { Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Weapons.StatMappings.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound) };
    }

    public Task<SaveEraWeaponTemplateResponse> SaveTemplateAsync(SaveEraWeaponTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (command.Template == null) throw Error(HttpStatusCode.BadRequest, "era.weapon_invalid_configuration", value => value.InvalidConfiguration);
            catalog.Templates.RemoveAll(row => row.Id == command.Template.Id);
            catalog.Templates.Add(command.Template);
            return catalog;
        }, cancellationToken);

    public Task<SaveEraWeaponModificationResponse> SaveModificationAsync(SaveEraWeaponModificationCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponModificationResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (command.Modification == null) throw Error(HttpStatusCode.BadRequest, "era.weapon_invalid_configuration", value => value.InvalidConfiguration);
            catalog.Modifications.RemoveAll(row => row.Id == command.Modification.Id);
            catalog.Modifications.Add(command.Modification);
            return catalog;
        }, cancellationToken);

    public Task<SaveEraWeaponStatMappingResponse> SaveStatMappingAsync(SaveEraWeaponStatMappingCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponStatMappingResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (command.StatMapping == null) throw Error(HttpStatusCode.BadRequest, "era.weapon_invalid_configuration", value => value.InvalidConfiguration);
            catalog.StatMappings.RemoveAll(row => row.Id == command.StatMapping.Id);
            catalog.StatMappings.Add(command.StatMapping);
            return catalog;
        }, cancellationToken);

    public Task<DisableEraWeaponTemplateResponse> DisableTemplateAsync(DisableEraWeaponTemplateCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DisableEraWeaponTemplateResponse>(command.ExpectedManifestVersion, catalog =>
        {
            var template = catalog.Templates.SingleOrDefault(row => row.Id == command.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            template.IsGenerationEnabled = false;
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

    public Task<DeleteEraWeaponStatMappingResponse> DeleteStatMappingAsync(DeleteEraWeaponStatMappingCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraWeaponStatMappingResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (!catalog.StatMappings.Any(row => row.Id == command.Id))
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            if (catalog.Templates.Any(row => row.StatMappingId == command.Id))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            catalog.StatMappings.RemoveAll(row => row.Id == command.Id);
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

    private async Task<T> WriteAsync<T>(long expectedVersion, Func<EraWeaponCatalogDto, EraWeaponCatalogDto> apply, CancellationToken cancellationToken)
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
        var proposed = apply(new EraWeaponCatalogDto
        {
            Templates = manifest.Weapons.Templates.ToList(), Modifications = manifest.Weapons.Modifications.ToList(),
            StatMappings = manifest.Weapons.StatMappings.ToList()
        });
        try { EraWeaponConfigurationValidator.Validate(proposed); }
        catch (ArgumentException) { throw Error(HttpStatusCode.BadRequest, "era.weapon_invalid_configuration", value => value.InvalidConfiguration); }
        await CheckReferencesAsync(proposed, manifest, cancellationToken).ConfigureAwait(false);
        await _weapons.ReplaceCatalogAsync(proposed, cancellationToken).ConfigureAwait(false);
        publication.Version++;
        publication.LastModifiedBy = _currentUser.Id;
        publication.UpdatedAt = _clock.GetUtcNow();
        return new T { Code = HttpStatusCode.OK, Data = new EraWeaponConfigurationResultDto { Version = publication.Version, Configuration = proposed } };
    }

    private async Task CheckReferencesAsync(EraWeaponCatalogDto proposed, EraEquipmentManifestDto manifest, CancellationToken cancellationToken)
    {
        if (proposed.Templates.Any(template => manifest.Definitions.Any(armor => armor.Id == template.Id)))
            throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
        var templates = proposed.Templates.ToDictionary(row => row.Id);
        var modifications = proposed.Modifications.ToDictionary(row => row.Id);
        var removedTemplates = manifest.Weapons.Templates.Select(row => row.Id).Except(templates.Keys).ToHashSet();
        if (removedTemplates.Count > 0)
        {
            var rewardPayload = await _weapons.GetRewardConfigurationPayloadAsync(cancellationToken).ConfigureAwait(false);
            if (rewardPayload != null)
            {
                EraRewardRules rewards;
                try
                {
                    rewards = System.Text.Json.JsonSerializer.Deserialize<EraRewardRules>(rewardPayload) ?? throw new ArgumentException("Reward configuration is missing.");
                    EraRewardConfigurationValidator.Validate(rewards);
                }
                catch (Exception exception) when (exception is ArgumentException or System.Text.Json.JsonException)
                {
                    throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable);
                }
                if (rewards.Pools.SelectMany(pool => pool.Entries).Any(entry => removedTemplates.Contains(entry.DefinitionId)))
                    throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            }
        }
        var equipment = await _weapons.GetReferencedWeaponsAsync(cancellationToken).ConfigureAwait(false);
        var candidates = await _weapons.GetReferencedCandidatesAsync(cancellationToken).ConfigureAwait(false);
        var items = equipment.ToDictionary(row => row.Id);
        if (equipment.Any(item => !templates.TryGetValue(item.DefinitionId, out var template) ||
                item.EquippedSlot.HasValue && !template.CompatibleSlots.Contains(item.EquippedSlot.Value)) || candidates.Any(candidate =>
            !modifications.TryGetValue(candidate.ModDefinitionId, out var modification) || modification.Slot != candidate.Slot))
            throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
        foreach (var candidate in candidates)
        {
            if (!items.TryGetValue(candidate.EquipmentId, out var item))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            var template = templates[item.DefinitionId];
            var modification = modifications[candidate.ModDefinitionId];
            if (modification.Modifiers.Any(value =>
                value.Stat == Void.Bot.Message.Enum.ERA.EraWeaponStat.ChargeTime && !template.BaseStats.ChargeTime.HasValue ||
                value.Stat == Void.Bot.Message.Enum.ERA.EraWeaponStat.ProjectileSpeed && !template.BaseStats.ProjectileSpeed.HasValue))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
        }
        // 已接受、尚未发放的计划同样固定了候选，不能在容量不足等待期间删掉其引用。
        var plans = await _weapons.GetPendingRewardPlansAsync(cancellationToken).ConfigureAwait(false);
        foreach (var payload in plans)
        {
            Void.Bot.Core.Services.ERA.Reward.Models.EraInventoryRewardPlan? plan;
            try { plan = System.Text.Json.JsonSerializer.Deserialize<Void.Bot.Core.Services.ERA.Reward.Models.EraInventoryRewardPlan>(payload); }
            catch (System.Text.Json.JsonException) { throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable); }
            if (plan?.Grants == null || plan.Grants.Any(grant => grant == null || grant.Equipment == null || grant.Equipment.Any(item => item == null)))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.weapon_config_unavailable", value => value.Unavailable);
            foreach (var item in plan.Grants.SelectMany(grant => grant.Equipment).Where(item => item.EquipmentType == Void.Bot.Message.Enum.ERA.EraEquipmentType.Weapon))
            {
                if (!templates.TryGetValue(item.DefinitionId, out var template) || !EraEquipmentItemRules.ValidWeapon(item.Weapon))
                    throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
                foreach (var slot in item.Weapon!.Slots)
                    foreach (var candidate in slot.Candidates)
                        if (!modifications.TryGetValue(candidate.ModDefinitionId, out var modification) || modification.Slot != slot.Slot ||
                            modification.Modifiers.Any(value => value.Stat == Void.Bot.Message.Enum.ERA.EraWeaponStat.ChargeTime && !template.BaseStats.ChargeTime.HasValue ||
                                value.Stat == Void.Bot.Message.Enum.ERA.EraWeaponStat.ProjectileSpeed && !template.BaseStats.ProjectileSpeed.HasValue))
                            throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            }
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
