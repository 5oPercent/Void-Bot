using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration.Models;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Configuration;

/// <summary>单类别配置的编辑规则；写入仅使用本类别的数据访问器，跨表只读校验由目录查询提供。</summary>
public abstract class EraDefinitionService<TRecord, TDefinition>(IEraDefinitionDataProvider<TRecord> records,
    IEraAbilityCatalogQueryService catalog, IEraConfigurationStateProvider states, ICurrentUser currentUser,
    ILocalizationService localization) : EraConfigurationServiceBase(localization)
    where TRecord : class, IEraDataDefinition, new()
{
    protected abstract EraDataKind Kind { get; }

    protected virtual void SetDefinition(TRecord record, TDefinition definition) => record.Payload = EraDataCatalog.Encode(definition);

    private static EraDataItem<TDefinition> Item(TRecord record, long revision) => new()
    {
        Id = record.StableId, Version = record.Version, Revision = revision,
        Definition = EraDataCatalog.Decode<TDefinition>(record)
    };

    private async Task<(EraDataEditState State, List<TRecord> Records)?> ReadAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var state = await states.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (state == null) return null;
            var definitions = await records.GetDefinitionsAsync(cancellationToken).ConfigureAwait(false);
            var after = await states.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (after?.Revision == state.Revision) return (state, definitions);
        }
        throw new InvalidOperationException("配置正在变化，请重试读取。");
    }

    protected async Task<TResponse> ListAsync<TResponse>(int skip, int take, string? search, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraDataPage<TDefinition>>, new()
    {
        try { ValidatePaging(skip, take); }
        catch (ArgumentException) { return DataInvalid<TResponse>(); }
        if (search?.Length > 256) return DataInvalid<TResponse>();
        var view = await ReadAsync(cancellationToken).ConfigureAwait(false);
        if (view == null) return DataUnavailable<TResponse>();
        var filtered = view.Value.Records.OrderBy(record => record.StableId, StringComparer.Ordinal).Where(record =>
        {
            if (string.IsNullOrWhiteSpace(search) || record.StableId.Contains(search, StringComparison.OrdinalIgnoreCase)) return true;
            using var json = JsonDocument.Parse(record.Payload);
            return (json.RootElement.GetProperty("Name").GetString() ?? "").Contains(search, StringComparison.OrdinalIgnoreCase);
        }).ToArray();
        return new TResponse
        {
            Code = HttpStatusCode.OK,
            Data = new() { Revision = view.Value.State.Revision, Total = filtered.Length,
                Items = filtered.Skip(skip).Take(take).Select(record => Item(record, view.Value.State.Revision)).ToList() }
        };
    }

    protected async Task<TResponse> GetAsync<TResponse>(string id, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraDataItem<TDefinition>>, new()
    {
        try { EraDataDefinitionValidator.Id(id); }
        catch (ArgumentException) { return DataInvalid<TResponse>(); }
        var view = await ReadAsync(cancellationToken).ConfigureAwait(false);
        if (view == null) return DataUnavailable<TResponse>();
        var record = view.Value.Records.SingleOrDefault(item => item.StableId == id);
        return record == null ? Error<TResponse>(HttpStatusCode.NotFound, "era.data_item_not_found", text => text.SkillNotFound)
            : new TResponse { Code = HttpStatusCode.OK, Data = Item(record, view.Value.State.Revision) };
    }

    protected async Task<TResponse> SaveAsync<TResponse>(string id, long expected, TDefinition definition, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraDataItem<TDefinition>>, new()
    {
        try
        {
            EraDataDefinitionValidator.Id(id);
            EraDataDefinitionValidator.Definition(Kind, definition!);
            if (id != EraDataDefinitionValidator.DefinitionId(definition!) || expected < 0 || expected == long.MaxValue)
                return DataInvalid<TResponse>();
        }
        catch (ArgumentException) { return DataInvalid<TResponse>(); }

        var state = await states.GetAsync(true, cancellationToken).ConfigureAwait(false);
        if (state == null) return DataUnavailable<TResponse>();
        var view = await catalog.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (view == null) return DataUnavailable<TResponse>();
        if (view.Workspace.Revision != state.Revision) return DataConflict<TResponse>();
        var record = await records.GetDefinitionAsync(id, true, cancellationToken).ConfigureAwait(false);
        if ((record?.Version ?? 0) != expected) return DataConflict<TResponse>();
        if (state.Revision == long.MaxValue || !HasDataBudget(view, JsonSerializer.Serialize(definition), record?.Payload))
            return DataInvalid<TResponse>();
        if (record == null && view.Records.Count(item => item.Kind == Kind) >= 10000)
            return DataInvalid<TResponse>();

        var candidate = new TRecord { Id = record?.Id ?? Guid.NewGuid(), StableId = id };
        SetDefinition(candidate, definition);
        var proposed = view.Records.Where(item => item.Kind != Kind || item.StableId != id).Append(candidate).ToList();
        if (EraDataCatalog.Validate(EraDataCatalog.Compile(new(view.Workspace, proposed))).Count != 0)
            return DataInvalid<TResponse>();

        var isNew = record == null;
        record ??= new TRecord { Id = candidate.Id, StableId = id };
        SetDefinition(record, definition);
        record.Version = checked(state.Revision + 1);
        record.LastModifiedBy = currentUser.Id;
        record.UpdatedAt = DateTimeOffset.UtcNow;
        if (isNew) await records.AddAsync(record, cancellationToken).ConfigureAwait(false);
        states.Touch(state);
        return new TResponse { Code = HttpStatusCode.OK, Data = Item(record, state.Revision) };
    }

    protected async Task<TResponse> DeleteAsync<TResponse>(string id, long expected, CancellationToken cancellationToken)
        where TResponse : EraResponse<EraDataItem<TDefinition>>, new()
    {
        try { EraDataDefinitionValidator.Id(id); }
        catch (ArgumentException) { return DataInvalid<TResponse>(); }
        if (expected < 1 || expected == long.MaxValue) return DataInvalid<TResponse>();
        var state = await states.GetAsync(true, cancellationToken).ConfigureAwait(false);
        if (state == null) return DataUnavailable<TResponse>();
        var record = await records.GetDefinitionAsync(id, true, cancellationToken).ConfigureAwait(false);
        if (record == null) return Error<TResponse>(HttpStatusCode.NotFound, "era.data_item_not_found", text => text.SkillNotFound);
        if (record.Version != expected) return DataConflict<TResponse>();
        if (state.Revision == long.MaxValue) return DataInvalid<TResponse>();
        var view = await catalog.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (view == null) return DataUnavailable<TResponse>();
        if (view.Workspace.Revision != state.Revision) return DataConflict<TResponse>();
        var proposed = view.Records.Where(item => item.Kind != Kind || item.StableId != id).ToList();
        if (EraDataCatalog.Validate(EraDataCatalog.Compile(new(view.Workspace, proposed))).Count != 0)
            return DataInvalid<TResponse>();
        await records.DeleteAsync(record, cancellationToken).ConfigureAwait(false);
        states.Touch(state);
        return new TResponse { Code = HttpStatusCode.OK, Data = Item(record, state.Revision) };
    }
}
