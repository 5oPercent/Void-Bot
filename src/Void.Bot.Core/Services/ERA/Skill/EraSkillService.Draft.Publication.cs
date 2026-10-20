using System.Net;
using System.Text.Json;
using Void.Bot.Core.Services.ERA.Skill.Models;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

public partial class EraSkillService
{
    private EraSkillDraftPreview DraftPreview(EraSkillDraftView view)
    {
        var configuration = EraSkillDraftCompiler.Compile(view);
        var published = DecodeConfiguration(view.Published);
        return new()
        {
            DraftRevision = view.Workspace.Revision,
            PublishedVersion = view.Published.Version,
            Errors = EraSkillDraftCompiler.Validate(configuration),
            Changes = EraSkillDraftCompiler.Changes(published, configuration),
            HasChanges = EraSkillDraftCompiler.Content(published) != EraSkillDraftCompiler.Content(configuration)
        };
    }

    public async Task<ValidateEraSkillDraftResponse> ValidateDraftAsync(ValidateEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        return view == null ? DraftUnavailable<ValidateEraSkillDraftResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftPreview(view)
        };
    }

    public async Task<PreviewEraSkillDraftResponse> PreviewDraftAsync(PreviewEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        return view == null ? DraftUnavailable<PreviewEraSkillDraftResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftPreview(view)
        };
    }

    public async Task<GetEraDraftReferencesResponse> GetDraftReferencesAsync(GetEraDraftReferencesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            SkillDraftDefinitionValidator.Id(request.Id);
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

        var configuration = EraSkillDraftCompiler.Compile(view);
        var references = new List<EraSkillDraftReference>();
        void Scan(IEnumerable<string> ids, string type, string id, string path)
        {
            if (ids.Contains(request.Id, StringComparer.Ordinal))
            {
                references.Add(new() { Type = type, Id = id, Path = path });
            }
        }

        if (request.Kind == EraSkillDraftKind.Skill)
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

        if (request.Kind == EraSkillDraftKind.Buff)
        {
            foreach (var skill in configuration.Skills)
            {
                foreach (var variant in skill.Variants)
                {
                    Scan(variant.BuffIds, "Skill", skill.Id, "Skills/" + skill.Id + "/Variants/" + variant.Slot + "/BuffIds");
                }
            }
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

    public async Task<ImportEraSkillDraftResponse> ImportDraftAsync(ImportEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.ExpectedDraftRevision < 0 || !Enum.IsDefined(command.Mode))
            {
                return DraftInvalid<ImportEraSkillDraftResponse>();
            }

            SkillConfigurationValidator.Validate(command.Configuration);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<ImportEraSkillDraftResponse>();
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<ImportEraSkillDraftResponse>();
        }

        if (view.Workspace.Revision != command.ExpectedDraftRevision)
        {
            return DraftConflict<ImportEraSkillDraftResponse>();
        }

        return await ImportIntoDraftAsync<ImportEraSkillDraftResponse>(view, command.Configuration, command.Mode, false, cancellationToken).ConfigureAwait(false);
    }

    private async Task<TResponse> ImportIntoDraftAsync<TResponse>(EraSkillDraftView view, SkillConfiguration configuration, EraSkillDraftImportMode mode, bool restoringHistory, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraSkillDraftStatus>, new()
    {
        var incoming = new List<(EraSkillDraftKind Kind, string Id, string Payload)>();
        void Add<T>(EraSkillDraftKind kind, T definition)
        {
            incoming.Add((kind, SkillDraftDefinitionValidator.DefinitionId(definition!), JsonSerializer.Serialize(definition)));
        }

        foreach (var definition in configuration.Systems)
        {
            Add(EraSkillDraftKind.System, definition);
        }

        foreach (var definition in configuration.Skills)
        {
            Add(EraSkillDraftKind.Skill, definition);
        }

        foreach (var definition in configuration.Buffs)
        {
            Add(EraSkillDraftKind.Buff, definition);
        }

        var existing = view.Records.ToDictionary(r => (r.Kind, r.StableId));
        if (!restoringHistory && incoming.Any(i => existing.TryGetValue((i.Kind, i.Id), out var r) && r.IsDeleted))
        {
            return Error<TResponse>(HttpStatusCode.Conflict, "era.draft_item_deleted", text => text.ItemDeleted);
        }

        foreach (var kind in Enum.GetValues<EraSkillDraftKind>())
        {
            if (existing.Keys.Where(k => k.Kind == kind).Select(k => k.StableId).Union(incoming.Where(i => i.Kind == kind).Select(i => i.Id), StringComparer.Ordinal).Count() > 10000)
            {
                return DraftInvalid<TResponse>();
            }
        }

        var oldSettings = JsonSerializer.Deserialize<EraSkillDraftSettings>(view.Workspace.SettingsPayload)!;
        var settings = mode == EraSkillDraftImportMode.Replace ? new EraSkillDraftSettings
        {
            CommonTacticalSkills = configuration.CommonTacticalSkills,
            DisabledSkills = configuration.DisabledSkills
        }

        : new EraSkillDraftSettings
        {
            CommonTacticalSkills = oldSettings.CommonTacticalSkills.Concat(configuration.CommonTacticalSkills).Distinct(StringComparer.Ordinal).ToList(),
            DisabledSkills = oldSettings.DisabledSkills.Concat(configuration.DisabledSkills).Distinct(StringComparer.Ordinal).ToList()
        };
        try
        {
            SkillDraftDefinitionValidator.Ids(settings.CommonTacticalSkills);
            SkillDraftDefinitionValidator.Ids(settings.DisabledSkills);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<TResponse>();
        }

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

        if (mode == EraSkillDraftImportMode.Replace)
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

    public async Task<PublishEraSkillDraftResponse> PublishDraftAsync(PublishEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.ExpectedDraftRevision < 0 || command.ExpectedPublishedVersion < 0 || command.ExpectedPublishedVersion == long.MaxValue)
        {
            return DraftInvalid<PublishEraSkillDraftResponse>();
        }

        if (!_currentUser.Id.HasValue || _currentUser.Id == Guid.Empty)
        {
            return Error<PublishEraSkillDraftResponse>(HttpStatusCode.Unauthorized, "era.unauthorized", text => text.InvalidDraft);
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<PublishEraSkillDraftResponse>();
        }

        var receipt = await _dataProvider.GetPublishReceiptAsync(command.RequestId, cancellationToken).ConfigureAwait(false);
        if (receipt != null)
        {
            if (receipt.AccountId != _currentUser.Id || receipt.ExpectedDraftRevision != command.ExpectedDraftRevision || receipt.ExpectedPublishedVersion != command.ExpectedPublishedVersion)
            {
                return Error<PublishEraSkillDraftResponse>(HttpStatusCode.Conflict, "era.publish_request_conflict", text => text.PublishRequestConflict);
            }

            var previous = await _dataProvider.GetPublicationAsync(receipt.PublishedVersion, cancellationToken).ConfigureAwait(false);
            return previous == null ? DraftUnavailable<PublishEraSkillDraftResponse>() : new()
            {
                Code = HttpStatusCode.OK,
                Data = JsonSerializer.Deserialize<SkillConfiguration>(previous.Payload)!
            };
        }

        if (view.Workspace.Revision != command.ExpectedDraftRevision || view.Published.Version != command.ExpectedPublishedVersion)
        {
            return DraftConflict<PublishEraSkillDraftResponse>();
        }

        var configuration = EraSkillDraftCompiler.Compile(view);
        if (EraSkillDraftCompiler.Validate(configuration).Count > 0)
        {
            return DraftInvalid<PublishEraSkillDraftResponse>();
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

        await _dataProvider.AddPublicationAsync(new() { Id = Guid.NewGuid(), Version = configuration.Version, DraftRevision = view.Workspace.Revision, Payload = payload, ContentHash = EraSkillDraftCompiler.Hash(configuration), PublishedBy = _currentUser.Id, PublishedAt = now }, cancellationToken).ConfigureAwait(false);
        await _dataProvider.AddPublishReceiptAsync(new() { Id = command.RequestId, AccountId = _currentUser.Id.Value, ExpectedDraftRevision = command.ExpectedDraftRevision, ExpectedPublishedVersion = command.ExpectedPublishedVersion, PublishedVersion = configuration.Version, CreatedAt = now }, cancellationToken).ConfigureAwait(false);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = configuration
        };
    }

    public async Task<ListEraSkillPublicationsResponse> ListPublicationsAsync(ListEraSkillPublicationsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            ValidatePaging(request.Skip, request.Take);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<ListEraSkillPublicationsResponse>();
        }

        var total = await _dataProvider.CountPublicationsAsync(cancellationToken).ConfigureAwait(false);
        var items = await _dataProvider.GetPublicationsAsync(request.Skip, request.Take, cancellationToken).ConfigureAwait(false);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                Total = total,
                Items = items.Select(p => new EraSkillPublicationInfo { Version = p.Version, DraftRevision = p.DraftRevision, ContentHash = p.ContentHash, PublishedBy = p.PublishedBy, PublishedAt = p.PublishedAt }).ToList()
            }
        };
    }

    public async Task<GetEraSkillPublicationResponse> GetPublicationAsync(GetEraSkillPublicationRequest request, CancellationToken cancellationToken)
    {
        if (request.Version < 0)
        {
            return DraftInvalid<GetEraSkillPublicationResponse>();
        }

        var record = await _dataProvider.GetPublicationAsync(request.Version, cancellationToken).ConfigureAwait(false);
        return record == null ? Error<GetEraSkillPublicationResponse>(HttpStatusCode.NotFound, "era.publication_not_found", text => text.ConfigUnavailable) : new()
        {
            Code = HttpStatusCode.OK,
            Data = JsonSerializer.Deserialize<SkillConfiguration>(record.Payload)!
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

        var configuration = JsonSerializer.Deserialize<SkillConfiguration>(record.Payload)!;
        SkillConfigurationValidator.Validate(configuration);
        return await ImportIntoDraftAsync<RestoreEraDraftPublicationResponse>(view, configuration, EraSkillDraftImportMode.Replace, true, cancellationToken).ConfigureAwait(false);
    }
}
