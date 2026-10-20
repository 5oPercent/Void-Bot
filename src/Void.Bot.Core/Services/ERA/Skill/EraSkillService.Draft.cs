using System.Net;
using System.Text.Json;
using Void.Bot.Core.Services.ERA.Skill.Models;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

public partial class EraSkillService
{
    private async Task<EraSkillDraftView?> ReadDraftAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            var workspace = await _dataProvider.GetDraftWorkspaceAsync(forUpdate, cancellationToken).ConfigureAwait(false);
            if (workspace?.Initialized != true)
            {
                return null;
            }

            var records = await _dataProvider.GetDraftRecordsAsync(forUpdate, cancellationToken).ConfigureAwait(false);
            var published = await _dataProvider.GetConfigurationAsync(forUpdate, cancellationToken).ConfigureAwait(false);
            if (published == null)
            {
                return null;
            }

            if (forUpdate)
            {
                return new(workspace, records, published);
            }

            var check = await _dataProvider.GetDraftWorkspaceAsync(false, cancellationToken).ConfigureAwait(false);
            if (check != null && check.Revision == workspace.Revision && check.BasePublishedVersion == workspace.BasePublishedVersion)
            {
                return new(workspace, records, published);
            }
        }

        throw new InvalidOperationException("草稿正在变化，请重试读取。");
    }

    private TResponse DraftUnavailable<TResponse>()
        where TResponse : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return Error<TResponse>(HttpStatusCode.ServiceUnavailable, "era.draft_unavailable", text => text.DraftUnavailable);
    }

    private TResponse DraftInvalid<TResponse>()
        where TResponse : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return Error<TResponse>(HttpStatusCode.BadRequest, "era.draft_invalid", text => text.InvalidDraft);
    }

    private TResponse DraftConflict<TResponse>()
        where TResponse : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return Error<TResponse>(HttpStatusCode.Conflict, "era.draft_version_conflict", text => text.VersionConflict);
    }

    private static bool HasDraftBudget(EraSkillDraftView view, string payload, string? replacing = null)
    {
        return System.Text.Encoding.UTF8.GetByteCount(payload) + view.Records.Sum(r => (long)System.Text.Encoding.UTF8.GetByteCount(r.Payload)) + System.Text.Encoding.UTF8.GetByteCount(view.Workspace.SettingsPayload) - (replacing == null ? 0 : System.Text.Encoding.UTF8.GetByteCount(replacing)) <= 1048576;
    }

    private static string DisplayName(EraSkillDraftRecord record)
    {
        using var json = JsonDocument.Parse(record.Payload);
        return json.RootElement.GetProperty("Name").GetString() ?? "";
    }

    private static EraSkillDraftItem<T> DraftItem<T>(EraSkillDraftRecord record, long revision)
    {
        return new()
        {
            Id = record.StableId,
            Version = record.Version,
            DraftRevision = revision,
            IsDeleted = record.IsDeleted,
            WasPublished = record.WasPublished,
            Definition = EraSkillDraftCompiler.Decode<T>(record)
        };
    }

    private EraSkillDraftStatus DraftStatus(EraSkillDraftView view)
    {
        return new()
        {
            Revision = view.Workspace.Revision,
            PublishedVersion = view.Published.Version,
            PublishedRevision = view.Workspace.PublishedRevision,
            SettingsVersion = view.Workspace.SettingsVersion,
            Settings = JsonSerializer.Deserialize<EraSkillDraftSettings>(view.Workspace.SettingsPayload)!,
            HasUnpublishedChanges = EraSkillDraftCompiler.Content(EraSkillDraftCompiler.Compile(view)) != EraSkillDraftCompiler.Content(DecodeConfiguration(view.Published))
        };
    }

    private void TouchDraft(EraSkillDraftWorkspace workspace)
    {
        workspace.Revision = checked(workspace.Revision + 1);
        workspace.LastModifiedBy = _currentUser.Id;
        workspace.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take is < 1 or > 100)
        {
            throw new ArgumentException("分页范围无效。");
        }
    }

    public async Task<GetEraSkillDraftResponse> GetDraftStatusAsync(GetEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        return view == null ? DraftUnavailable<GetEraSkillDraftResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftStatus(view)
        };
    }

    public async Task<InitializeEraSkillDraftResponse> InitializeDraftAsync(InitializeEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _dataProvider.GetDraftWorkspaceAsync(true, cancellationToken).ConfigureAwait(false);
        if (workspace == null)
        {
            return DraftUnavailable<InitializeEraSkillDraftResponse>();
        }

        if (workspace.Initialized)
        {
            var existing = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
            return existing == null ? DraftUnavailable<InitializeEraSkillDraftResponse>() : new()
            {
                Code = HttpStatusCode.OK,
                Data = DraftStatus(existing)
            };
        }

        var published = await _dataProvider.GetConfigurationAsync(true, cancellationToken).ConfigureAwait(false);
        if (published == null)
        {
            return DraftUnavailable<InitializeEraSkillDraftResponse>();
        }

        var configuration = DecodeConfiguration(published);
        var records = new List<EraSkillDraftRecord>();
        async Task Add<T>(EraSkillDraftKind kind, T definition)
        {
            var record = new EraSkillDraftRecord
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                Kind = kind,
                StableId = SkillDraftDefinitionValidator.DefinitionId(definition!),
                Version = 1,
                Payload = JsonSerializer.Serialize(definition),
                WasPublished = true,
                LastModifiedBy = _currentUser.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            records.Add(record);
            await _dataProvider.AddDraftRecordAsync(record, cancellationToken).ConfigureAwait(false);
        }

        foreach (var definition in configuration.Systems)
        {
            await Add(EraSkillDraftKind.System, definition);
        }

        foreach (var definition in configuration.Skills)
        {
            await Add(EraSkillDraftKind.Skill, definition);
        }

        foreach (var definition in configuration.Buffs)
        {
            await Add(EraSkillDraftKind.Buff, definition);
        }

        workspace.SettingsPayload = JsonSerializer.Serialize(new EraSkillDraftSettings { CommonTacticalSkills = configuration.CommonTacticalSkills, DisabledSkills = configuration.DisabledSkills });
        workspace.SettingsVersion = 1;
        workspace.Initialized = true;
        TouchDraft(workspace);
        workspace.BasePublishedVersion = published.Version;
        workspace.PublishedRevision = workspace.Revision;
        if (await _dataProvider.GetPublicationAsync(published.Version, cancellationToken).ConfigureAwait(false) == null)
        {
            await _dataProvider.AddPublicationAsync(new() { Id = Guid.NewGuid(), Version = published.Version, DraftRevision = workspace.Revision, Payload = JsonSerializer.Serialize(configuration), ContentHash = EraSkillDraftCompiler.Hash(configuration), PublishedBy = published.LastModifiedBy, PublishedAt = published.UpdatedAt ?? DateTimeOffset.UtcNow }, cancellationToken).ConfigureAwait(false);
        }

        return new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftStatus(new(workspace, records, published))
        };
    }

    public async Task<ExportEraSkillDraftResponse> ExportDraftAsync(ExportEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        return view == null ? DraftUnavailable<ExportEraSkillDraftResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = EraSkillDraftCompiler.Compile(view)
        };
    }

    private async Task<TResponse> ListDraftItemsAsync<TResponse, T>(EraSkillDraftKind kind, int skip, int take, string? search, bool includeDeleted, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraSkillDraftPage<T>>, new()
    {
        try
        {
            ValidatePaging(skip, take);
            if (search?.Length > 256)
            {
                return DraftInvalid<TResponse>();
            }
        }
        catch (ArgumentException)
        {
            return DraftInvalid<TResponse>();
        }

        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<TResponse>();
        }

        var items = view.Records.Where(r => r.Kind == kind && (includeDeleted || !r.IsDeleted)).OrderBy(r => r.StableId, StringComparer.Ordinal);
        var filtered = items.Where(r => string.IsNullOrWhiteSpace(search) || r.StableId.Contains(search, StringComparison.OrdinalIgnoreCase) || DisplayName(r).Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray();
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                DraftRevision = view.Workspace.Revision,
                Total = filtered.Length,
                Items = filtered.Skip(skip).Take(take).Select(r => DraftItem<T>(r, view.Workspace.Revision)).ToList()
            }
        };
    }

    private async Task<TResponse> GetDraftItemAsync<TResponse, T>(EraSkillDraftKind kind, string id, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraSkillDraftItem<T>>, new()
    {
        try
        {
            SkillDraftDefinitionValidator.Id(id);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<TResponse>();
        }

        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<TResponse>();
        }

        var record = view.Records.SingleOrDefault(r => r.Kind == kind && r.StableId == id);
        return record == null ? Error<TResponse>(HttpStatusCode.NotFound, "era.draft_item_not_found", text => text.SkillNotFound) : new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftItem<T>(record, view.Workspace.Revision)
        };
    }

    private async Task<TResponse> SaveDraftItemAsync<TResponse, T>(EraSkillDraftKind kind, string id, long expected, T definition, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraSkillDraftItem<T>>, new()
    {
        try
        {
            SkillDraftDefinitionValidator.Id(id);
            SkillDraftDefinitionValidator.Definition(kind, definition!);
            if (id != SkillDraftDefinitionValidator.DefinitionId(definition!) || expected < 0 || expected == long.MaxValue)
            {
                return DraftInvalid<TResponse>();
            }
        }
        catch (ArgumentException)
        {
            return DraftInvalid<TResponse>();
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<TResponse>();
        }

        var record = view.Records.SingleOrDefault(r => r.Kind == kind && r.StableId == id);
        if ((record?.Version ?? 0) != expected)
        {
            return DraftConflict<TResponse>();
        }

        if (view.Workspace.Revision == long.MaxValue || !HasDraftBudget(view, JsonSerializer.Serialize(definition), record?.Payload))
        {
            return DraftInvalid<TResponse>();
        }

        if (record?.IsDeleted == true)
        {
            return Error<TResponse>(HttpStatusCode.Conflict, "era.draft_item_deleted", text => text.ItemDeleted);
        }

        if (record == null && view.Records.Count(r => r.Kind == kind) >= 10000)
        {
            return DraftInvalid<TResponse>();
        }

        if (record == null)
        {
            record = new()
            {
                Id = Guid.NewGuid(),
                WorkspaceId = view.Workspace.Id,
                Kind = kind,
                StableId = id
            };
            await _dataProvider.AddDraftRecordAsync(record, cancellationToken).ConfigureAwait(false);
        }

        record.Payload = JsonSerializer.Serialize(definition);
        record.Version = checked(record.Version + 1);
        record.LastModifiedBy = _currentUser.Id;
        record.UpdatedAt = DateTimeOffset.UtcNow;
        TouchDraft(view.Workspace);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftItem<T>(record, view.Workspace.Revision)
        };
    }

    private async Task<TResponse> ChangeDraftDeletionAsync<TResponse, T>(EraSkillDraftKind kind, string id, long expected, bool deleted, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraSkillDraftItem<T>>, new()
    {
        try
        {
            SkillDraftDefinitionValidator.Id(id);
            if (expected < 1 || expected == long.MaxValue)
            {
                return DraftInvalid<TResponse>();
            }
        }
        catch (ArgumentException)
        {
            return DraftInvalid<TResponse>();
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<TResponse>();
        }

        var record = view.Records.SingleOrDefault(r => r.Kind == kind && r.StableId == id);
        if (record == null)
        {
            return Error<TResponse>(HttpStatusCode.NotFound, "era.draft_item_not_found", text => text.SkillNotFound);
        }

        if (record.Version != expected)
        {
            return DraftConflict<TResponse>();
        }

        if (view.Workspace.Revision == long.MaxValue)
        {
            return DraftInvalid<TResponse>();
        }

        if (record.IsDeleted != deleted)
        {
            record.IsDeleted = deleted;
            record.Version = checked(record.Version + 1);
            record.LastModifiedBy = _currentUser.Id;
            record.UpdatedAt = DateTimeOffset.UtcNow;
            TouchDraft(view.Workspace);
        }

        return new()
        {
            Code = HttpStatusCode.OK,
            Data = DraftItem<T>(record, view.Workspace.Revision)
        };
    }

    public async Task<GetEraDraftSettingsResponse> GetDraftSettingsAsync(GetEraDraftSettingsRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadDraftAsync(false, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<GetEraDraftSettingsResponse>();
        }

        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                Version = view.Workspace.SettingsVersion,
                DraftRevision = view.Workspace.Revision,
                Settings = JsonSerializer.Deserialize<EraSkillDraftSettings>(view.Workspace.SettingsPayload)!
            }
        };
    }

    public async Task<SaveEraDraftSettingsResponse> SaveDraftSettingsAsync(SaveEraDraftSettingsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.ExpectedVersion < 0 || command.ExpectedVersion == long.MaxValue || command.Settings == null)
            {
                return DraftInvalid<SaveEraDraftSettingsResponse>();
            }

            SkillDraftDefinitionValidator.Ids(command.Settings.CommonTacticalSkills);
            SkillDraftDefinitionValidator.Ids(command.Settings.DisabledSkills);
        }
        catch (ArgumentException)
        {
            return DraftInvalid<SaveEraDraftSettingsResponse>();
        }

        var view = await ReadDraftAsync(true, cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DraftUnavailable<SaveEraDraftSettingsResponse>();
        }

        if (view.Workspace.SettingsVersion != command.ExpectedVersion)
        {
            return DraftConflict<SaveEraDraftSettingsResponse>();
        }

        if (view.Workspace.Revision == long.MaxValue || !HasDraftBudget(view, JsonSerializer.Serialize(command.Settings), view.Workspace.SettingsPayload))
        {
            return DraftInvalid<SaveEraDraftSettingsResponse>();
        }

        view.Workspace.SettingsPayload = JsonSerializer.Serialize(command.Settings);
        view.Workspace.SettingsVersion = checked(view.Workspace.SettingsVersion + 1);
        TouchDraft(view.Workspace);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                Version = view.Workspace.SettingsVersion,
                DraftRevision = view.Workspace.Revision,
                Settings = command.Settings
            }
        };
    }
}
