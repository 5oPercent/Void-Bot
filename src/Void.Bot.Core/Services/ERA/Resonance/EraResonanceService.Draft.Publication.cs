using System.Net;
using System.Text.Json;
using Void.Bot.Core.Services.ERA.Resonance.Models;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial class EraResonanceService
{
    private EraResonanceDraftPreview DraftPreview(EraResonanceDraftView view)
    {
        var configuration = EraResonanceDraftCompiler.Compile(view);
        var published = DecodeConfiguration(view.Published);
        return new()
        {
            DraftRevision = view.Workspace.Revision,
            PublishedVersion = view.Published.Version,
            Errors = EraResonanceDraftCompiler.Validate(configuration),
            Changes = EraResonanceDraftCompiler.Changes(published, configuration),
            HasChanges = EraResonanceDraftCompiler.Content(published) != EraResonanceDraftCompiler.Content(configuration)
        };
    }

    public async Task<ValidateEraResonanceDraftResponse> ValidateDraftAsync(ValidateEraResonanceDraftRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        return view == null ? DraftUnavailable<ValidateEraResonanceDraftResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftPreview(view)
        };
    }

    public async Task<PreviewEraResonanceDraftResponse> PreviewDraftAsync(PreviewEraResonanceDraftRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        return view == null ? DraftUnavailable<PreviewEraResonanceDraftResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftPreview(view)
        };
    }

    public async Task<GetEraDraftReferencesResponse> GetDraftReferencesAsync(GetEraDraftReferencesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            ResonanceDraftDefinitionValidator.Id(request.Id);
            ValidatePaging(request.Skip, request.Take);
            if (!Enum.IsDefined(request.Kind))
            {
                return DraftInvalid<GetEraDraftReferencesResponse>();
            }
        }
        catch (ArgumentException)
        {
            return DraftInvalid<GetEraDraftReferencesResponse>();
        }

        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<GetEraDraftReferencesResponse>();
        }

        var configuration = EraResonanceDraftCompiler.Compile(view);
        var references = new List<EraResonanceDraftReference>();
        void Scan(IEnumerable<string> ids, string type, string id, string path)
        {
            if (ids.Contains(request.Id, StringComparer.Ordinal))
            {
                references.Add(new() { Type = type, Id = id, Path = path });
            }
        }

        if (request.Kind == EraResonanceDraftKind.Resonance)
        {
            foreach (var system in configuration.Systems)
            {
                Scan(system.SpecialSkills, "System", system.Id, "Systems/" + system.Id + "/SpecialSkills");
                Scan(system.PrimarySkills, "System", system.Id, "Systems/" + system.Id + "/PrimarySkills");
                Scan(system.SecondarySkills, "System", system.Id, "Systems/" + system.Id + "/SecondarySkills");
                Scan(system.TacticalSkills, "System", system.Id, "Systems/" + system.Id + "/TacticalSkills");
            }

            Scan(configuration.CommonTacticalSkills, "Settings", "CommonTacticalSkills", "Settings/CommonTacticalSkills");
            Scan(configuration.DisabledSkills, "Settings", "DisabledSkills", "Settings/DisabledSkills");
        }

        if (request.Kind == EraResonanceDraftKind.Buff)
        {
            foreach (var skill in configuration.Skills)
            {
                foreach (var variant in skill.Variants)
                {
                    Scan(variant.Effects.Select(effect => effect.EffectId), "Resonance", skill.Id, "Skills/" + skill.Id + "/Variants/" + variant.Slot + "/Effects");
                }
            }
        }

        foreach (var enemy in configuration.Enemies ?? new())
        {
            if (request.Kind == EraResonanceDraftKind.Resonance) Scan(enemy.Skills.Select(s => s.SkillId), "Enemy", enemy.Id, "Enemies/" + enemy.Id + "/Skills");
            if (request.Kind == EraResonanceDraftKind.Buff) Scan(enemy.InitialBuffs, "Enemy", enemy.Id, "Enemies/" + enemy.Id + "/InitialBuffs");
        }

        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                DraftRevision = view.Workspace.Revision,
                Total = references.Count,
                Items = references.Skip(request.Skip).Take(request.Take).ToList()
            }
        };
    }

    public async Task<ImportEraResonanceDraftResponse> ImportDraftAsync(ImportEraResonanceDraftCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.ExpectedDraftRevision < 0 || !Enum.IsDefined(command.Mode))
            {
                return DraftInvalid<ImportEraResonanceDraftResponse>();
            }

            ResonanceConfigurationValidator.Validate(command.Configuration);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<ImportEraResonanceDraftResponse>();
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<ImportEraResonanceDraftResponse>();
        }

        if (view.Workspace.Revision != command.ExpectedDraftRevision)
        {
            return DraftConflict<ImportEraResonanceDraftResponse>();
        }

        return await ImportIntoDraftAsync<ImportEraResonanceDraftResponse>(view, command.Configuration, command.Mode, false, cancellationToken).ConfigureAwait(false);
    }

    private async Task<TResponse> ImportIntoDraftAsync<TResponse>(EraResonanceDraftView view, ResonanceConfiguration configuration, EraResonanceDraftImportMode mode, bool restoringHistory, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraResonanceDraftStatus>, new()
    {
        var incoming = new List<(EraResonanceDraftKind Kind, string Id, string Payload)>();
        void Add<T>(EraResonanceDraftKind kind, T definition)
        {
            incoming.Add((kind, ResonanceDraftDefinitionValidator.DefinitionId(definition!), JsonSerializer.Serialize(definition)));
        }

        foreach (var definition in configuration.Systems)
        {
            Add(EraResonanceDraftKind.Lattice, definition);
        }

        foreach (var definition in configuration.Skills)
        {
            Add(EraResonanceDraftKind.Resonance, definition);
        }

        foreach (var definition in configuration.Buffs)
        {
            Add(EraResonanceDraftKind.Buff, definition);
        }

        // Older imports omit this optional collection: preserve enemies. Restoring older history intentionally clears it.
        foreach (var definition in configuration.Enemies ?? (restoringHistory ? new List<EnemyDefinition>() : view.Records.Where(r => r.Kind == EraResonanceDraftKind.Enemy && !r.IsDeleted).Select(EraResonanceDraftCompiler.Decode<EnemyDefinition>).ToList()))
        {
            Add(EraResonanceDraftKind.Enemy, definition);
        }

        var existing = view.Records.ToDictionary(r => (r.Kind, r.StableId));
        if (!restoringHistory && incoming.Any(i => existing.TryGetValue((i.Kind, i.Id), out var r) && r.IsDeleted))
        {
            return Error<TResponse>(HttpStatusCode.Conflict, "era.draft_item_deleted", text => text.ItemDeleted);
        }

        foreach (var kind in Enum.GetValues<EraResonanceDraftKind>())
        {
            if (existing.Keys.Where(k => k.Kind == kind).Select(k => k.StableId).Union(incoming.Where(i => i.Kind == kind).Select(i => i.Id), StringComparer.Ordinal).Count() > 10000)
            {
                return DraftInvalid<TResponse>();
            }
        }

        var oldSettings = JsonSerializer.Deserialize<EraResonanceDraftSettings>(view.Workspace.SettingsPayload)!;
        var settings = mode == EraResonanceDraftImportMode.Replace ? new EraResonanceDraftSettings
        {
            CombatRelations = configuration.CombatRelations ?? oldSettings.CombatRelations,
            CommonTacticalSkills = configuration.CommonTacticalSkills,
            DisabledSkills = configuration.DisabledSkills
        }

        : new EraResonanceDraftSettings
        {
            CombatRelations = configuration.CombatRelations ?? oldSettings.CombatRelations,
            CommonTacticalSkills = oldSettings.CommonTacticalSkills.Concat(configuration.CommonTacticalSkills).Distinct(StringComparer.Ordinal).ToList(),
            DisabledSkills = oldSettings.DisabledSkills.Concat(configuration.DisabledSkills).Distinct(StringComparer.Ordinal).ToList()
        };
        try
        {
            ResonanceDraftDefinitionValidator.Ids(settings.CommonTacticalSkills);
            ResonanceDraftDefinitionValidator.Ids(settings.DisabledSkills);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<TResponse>();
        }

        var combined = new ResonanceConfiguration { CombatRelations = settings.CombatRelations, CommonTacticalSkills = settings.CommonTacticalSkills, DisabledSkills = settings.DisabledSkills };
        var effective = incoming.ToDictionary(i => (i.Kind, i.Id), i => i.Payload);
        if (mode == EraResonanceDraftImportMode.Merge)
            foreach (var record in view.Records.Where(r => !r.IsDeleted)) effective.TryAdd((record.Kind, record.StableId), record.Payload);
        foreach (var record in effective)
        {
            switch (record.Key.Kind)
            {
                case EraResonanceDraftKind.Lattice: combined.Systems.Add(JsonSerializer.Deserialize<LatticeDefinition>(record.Value)!); break;
                case EraResonanceDraftKind.Resonance: combined.Skills.Add(JsonSerializer.Deserialize<ResonanceDefinition>(record.Value)!); break;
                case EraResonanceDraftKind.Buff: combined.Buffs.Add(JsonSerializer.Deserialize<BuffDefinitionDto>(record.Value)!); break;
                case EraResonanceDraftKind.Enemy: (combined.Enemies ??= new()).Add(JsonSerializer.Deserialize<EnemyDefinition>(record.Value)!); break;
            }
        }
        try { ResonanceConfigurationValidator.Validate(combined); }
        catch (ArgumentException) { return DraftInvalid<TResponse>(); }
        var settingsPayload = JsonSerializer.Serialize(settings);
        long bytes = System.Text.Encoding.UTF8.GetByteCount(settingsPayload) + view.Records.Sum(r => (long)System.Text.Encoding.UTF8.GetByteCount(r.Payload));
        foreach (var item in incoming)
        {
            bytes += System.Text.Encoding.UTF8.GetByteCount(item.Payload) - (existing.TryGetValue((item.Kind, item.Id), out var old) ? System.Text.Encoding.UTF8.GetByteCount(old.Payload) : 0);
        }

        if (bytes > 1048576 || view.Records.Any(r => r.Version == long.MaxValue) || view.Workspace.SettingsVersion == long.MaxValue || view.Workspace.Revision == long.MaxValue)
        {
            return DraftInvalid<TResponse>();
        }

        // 所有业务错误在修改跟踪实体之前判定，避免失败响应仍被工作单元提交。
        foreach (var item in incoming)
        {
            if (!existing.TryGetValue((item.Kind, item.Id), out var record))
            {
                record = new()
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = view.Workspace.Id,
                    Kind = item.Kind,
                    StableId = item.Id
                };
                view.Records.Add(record);
                existing.Add((item.Kind, item.Id), record);
                await _dataProvider.AddDraftRecordAsync(record, cancellationToken).ConfigureAwait(false);
            }

            record.Payload = item.Payload;
            record.IsDeleted = false;
            record.Version = checked(record.Version + 1);
            record.LastModifiedBy = _currentUser.Id;
            record.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (mode == EraResonanceDraftImportMode.Replace)
        {
            var included = incoming.Select(i => (i.Kind, i.Id)).ToHashSet();
            foreach (var record in view.Records.Where(r => !r.IsDeleted && !included.Contains((r.Kind, r.StableId))))
            {
                record.IsDeleted = true;
                record.Version = checked(record.Version + 1);
                record.LastModifiedBy = _currentUser.Id;
                record.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        view.Workspace.SettingsPayload = settingsPayload;
        view.Workspace.SettingsVersion = checked(view.Workspace.SettingsVersion + 1);
        TouchDraft(view.Workspace);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftStatus(view)
        };
    }

    public async Task<PublishEraResonanceDraftResponse> PublishDraftAsync(PublishEraResonanceDraftCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.ExpectedDraftRevision < 0 || command.ExpectedPublishedVersion < 0 || command.ExpectedPublishedVersion == long.MaxValue)
        {
            return DraftInvalid<PublishEraResonanceDraftResponse>();
        }

        if (!_currentUser.Id.HasValue || _currentUser.Id == Guid.Empty)
        {
            return Error<PublishEraResonanceDraftResponse>(HttpStatusCode.Unauthorized, "era.unauthorized", text => text.InvalidDraft);
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<PublishEraResonanceDraftResponse>();
        }

        var receipt = await _dataProvider.GetPublishReceiptAsync(command.RequestId, cancellationToken).ConfigureAwait(false);
        if (receipt != null)
        {
            if (receipt.AccountId != _currentUser.Id || receipt.ExpectedDraftRevision != command.ExpectedDraftRevision || receipt.ExpectedPublishedVersion != command.ExpectedPublishedVersion)
            {
                return Error<PublishEraResonanceDraftResponse>(HttpStatusCode.Conflict, "era.publish_request_conflict", text => text.PublishRequestConflict);
            }

            var previous = await _dataProvider.GetPublicationAsync(receipt.PublishedVersion, cancellationToken).ConfigureAwait(false);
            return previous == null ? DraftUnavailable<PublishEraResonanceDraftResponse>() : new()
            {
                Code = HttpStatusCode.OK,
                Data = EraResonanceDraftCompiler.DecodePublication(previous.Payload)
            };
        }

        if (view.Workspace.Revision != command.ExpectedDraftRevision || view.Published.Version != command.ExpectedPublishedVersion)
        {
            return DraftConflict<PublishEraResonanceDraftResponse>();
        }

        var configuration = EraResonanceDraftCompiler.Compile(view);
        if (EraResonanceDraftCompiler.Validate(configuration).Count > 0)
        {
            return DraftInvalid<PublishEraResonanceDraftResponse>();
        }

        configuration.Version = checked(view.Published.Version + 1);
        var payload = JsonSerializer.Serialize(configuration);
        var now = DateTimeOffset.UtcNow;
        view.Published.Version = configuration.Version;
        view.Published.Payload = payload;
        view.Published.LastModifiedBy = _currentUser.Id;
        view.Workspace.BasePublishedVersion = configuration.Version;
        view.Workspace.PublishedRevision = view.Workspace.Revision;
        foreach (var record in view.Records.Where(r => !r.IsDeleted))
        {
            record.WasPublished = true;
        }

        await _dataProvider.AddPublicationAsync(new() { Id = Guid.NewGuid(), Version = configuration.Version, DraftRevision = view.Workspace.Revision, Payload = payload, ContentHash = EraResonanceDraftCompiler.Hash(configuration), PublishedBy = _currentUser.Id, PublishedAt = now }, cancellationToken).ConfigureAwait(false);
        await _dataProvider.AddPublishReceiptAsync(new() { Id = command.RequestId, AccountId = _currentUser.Id.Value, ExpectedDraftRevision = command.ExpectedDraftRevision, ExpectedPublishedVersion = command.ExpectedPublishedVersion, PublishedVersion = configuration.Version, CreatedAt = now }, cancellationToken).ConfigureAwait(false);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = configuration
        };
    }

    public async Task<ListEraResonancePublicationsResponse> ListPublicationsAsync(ListEraResonancePublicationsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            ValidatePaging(request.Skip, request.Take);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<ListEraResonancePublicationsResponse>();
        }

        var total = await _dataProvider.CountPublicationsAsync(cancellationToken).ConfigureAwait(false);
        var items = await _dataProvider.GetPublicationsAsync(request.Skip, request.Take, cancellationToken).ConfigureAwait(false);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                Total = total,
                Items = items.Select(p => new EraResonancePublicationInfo { Version = p.Version, DraftRevision = p.DraftRevision, ContentHash = p.ContentHash, PublishedBy = p.PublishedBy, PublishedAt = p.PublishedAt }).ToList()
            }
        };
    }

    public async Task<GetEraResonancePublicationResponse> GetPublicationAsync(GetEraResonancePublicationRequest request, CancellationToken cancellationToken)
    {
        if (request.Version < 0)
        {
            return DraftInvalid<GetEraResonancePublicationResponse>();
        }

        var record = await _dataProvider.GetPublicationAsync(request.Version, cancellationToken).ConfigureAwait(false);
        return record == null ? Error<GetEraResonancePublicationResponse>(HttpStatusCode.NotFound, "era.publication_not_found", text => text.ConfigUnavailable) : new()
        {
            Code = HttpStatusCode.OK,
            Data = EraResonanceDraftCompiler.DecodePublication(record.Payload)
        };
    }

    public async Task<RestoreEraDraftPublicationResponse> RestorePublicationToDraftAsync(RestoreEraDraftPublicationCommand command, CancellationToken cancellationToken)
    {
        if (command.Version < 0 || command.ExpectedDraftRevision < 0)
        {
            return DraftInvalid<RestoreEraDraftPublicationResponse>();
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<RestoreEraDraftPublicationResponse>();
        }

        if (view.Workspace.Revision != command.ExpectedDraftRevision)
        {
            return DraftConflict<RestoreEraDraftPublicationResponse>();
        }

        var record = await _dataProvider.GetPublicationAsync(command.Version, cancellationToken).ConfigureAwait(false);
        if (record == null)
        {
            return Error<RestoreEraDraftPublicationResponse>(HttpStatusCode.NotFound, "era.publication_not_found", text => text.ConfigUnavailable);
        }

        var configuration = EraResonanceDraftCompiler.DecodePublication(record.Payload);
        try { ResonanceConfigurationValidator.Validate(configuration); }
        catch (ArgumentException) { return DraftInvalid<RestoreEraDraftPublicationResponse>(); }
        return await ImportIntoDraftAsync<RestoreEraDraftPublicationResponse>(view, configuration, EraResonanceDraftImportMode.Replace, true, cancellationToken).ConfigureAwait(false);
    }
}
