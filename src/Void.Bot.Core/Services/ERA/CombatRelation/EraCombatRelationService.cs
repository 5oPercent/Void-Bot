using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.CombatRelation;

public interface IEraCombatRelationService : IScope
{
    Task<GetEraDataSettingsResponse> GetDataSettingsAsync(GetEraDataSettingsRequest request, CancellationToken cancellationToken);
    Task<SaveEraDataSettingsResponse> SaveDataSettingsAsync(SaveEraDataSettingsCommand command, CancellationToken cancellationToken);
}

/// <summary>仅管理种族、阵营及默认关系，不管理技能或其它类别定义。</summary>
public sealed class EraCombatRelationService(IEraCombatRelationDataProvider relations, IEraAbilityCatalogQueryService catalog,
    IEraConfigurationStateProvider states, ILocalizationService localization) : EraConfigurationServiceBase(localization), IEraCombatRelationService
{
    private readonly IEraCombatRelationDataProvider _relations = relations;
    private readonly IEraAbilityCatalogQueryService _catalog = catalog;
    private readonly IEraConfigurationStateProvider _states = states;

    public async Task<GetEraDataSettingsResponse> GetDataSettingsAsync(GetEraDataSettingsRequest request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var state = await _states.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (state == null) return DataUnavailable<GetEraDataSettingsResponse>();
            var record = await _relations.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (record == null) return DataUnavailable<GetEraDataSettingsResponse>();
            var after = await _states.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (after?.Revision != state.Revision) continue;
            return new()
            {
                Code = HttpStatusCode.OK,
                Data = new()
                {
                    Version = state.SettingsVersion,
                    Revision = state.Revision,
                    Settings = new() { CombatRelations = JsonSerializer.Deserialize<CombatRelationConfigurationDto>(record.Payload) }
                }
            };
        }
        throw new InvalidOperationException("配置正在变化，请重试读取。");
    }

    public async Task<SaveEraDataSettingsResponse> SaveDataSettingsAsync(SaveEraDataSettingsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.ExpectedVersion < 0 || command.ExpectedVersion == long.MaxValue || command.Settings == null)
            {
                return DataInvalid<SaveEraDataSettingsResponse>();
            }

            CombatRelationConfigurationValidator.Validate(command.Settings.CombatRelations);
        }
        catch (ArgumentException)
        {
            return DataInvalid<SaveEraDataSettingsResponse>();
        }

        var state = await _states.GetAsync(true, cancellationToken).ConfigureAwait(false);
        if (state == null) return DataUnavailable<SaveEraDataSettingsResponse>();
        var view = await _catalog.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DataUnavailable<SaveEraDataSettingsResponse>();
        }
        if (view.Workspace.Revision != state.Revision) return DataConflict<SaveEraDataSettingsResponse>();
        state.SettingsPayload = view.Workspace.SettingsPayload;
        view = new(state, view.Records);

        if (view.Workspace.SettingsVersion != command.ExpectedVersion)
        {
            return DataConflict<SaveEraDataSettingsResponse>();
        }

        command.Settings.CombatRelations ??= JsonSerializer.Deserialize<EraDataSettings>(view.Workspace.SettingsPayload)!.CombatRelations;
        if (view.Workspace.Revision == long.MaxValue || !HasDataBudget(view, JsonSerializer.Serialize(command.Settings), view.Workspace.SettingsPayload))
        {
            return DataInvalid<SaveEraDataSettingsResponse>();
        }

        var candidateState = new EraDataEditState { SettingsPayload = JsonSerializer.Serialize(command.Settings) };
        if (EraDataCatalog.Validate(EraDataCatalog.Compile(new(candidateState, view.Records))).Count != 0)
            return DataInvalid<SaveEraDataSettingsResponse>();
        var record = await _relations.GetAsync(true, cancellationToken).ConfigureAwait(false);
        if (record == null) return DataUnavailable<SaveEraDataSettingsResponse>();
        record.Payload = JsonSerializer.Serialize(command.Settings.CombatRelations ?? new());
        view.Workspace.SettingsPayload = candidateState.SettingsPayload;
        view.Workspace.SettingsVersion = checked(view.Workspace.SettingsVersion + 1);
        _states.Touch(view.Workspace);
        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                Version = view.Workspace.SettingsVersion,
                Revision = view.Workspace.Revision,
                Settings = command.Settings
            }
        };
    }
}
