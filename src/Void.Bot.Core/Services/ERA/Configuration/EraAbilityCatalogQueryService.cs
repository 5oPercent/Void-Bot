using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration.Models;
using Void.Bot.Core.Services.ERA.Lattice;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Core.Services.ERA.Effect;
using Void.Bot.Core.Services.ERA.Enemy;
using Void.Bot.Core.Services.ERA.CombatRelation;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Configuration;

/// <summary>只读聚合各类别的当前配置及引用，不提供保存或删除入口。</summary>
public interface IEraAbilityCatalogQueryService : IScope
{
    Task<EraDataView?> ReadAsync(CancellationToken cancellationToken);
    Task<ResonanceConfiguration?> ReadConfigurationAsync(CancellationToken cancellationToken);
    Task<GetEraResonanceConfigurationResponse> GetConfigurationAsync(GetEraResonanceConfigurationRequest request, CancellationToken cancellationToken);
    Task<GetEraResonanceDataResponse> GetDataStatusAsync(GetEraResonanceDataRequest request, CancellationToken cancellationToken);
    Task<ExportEraResonanceDataResponse> ExportDataAsync(ExportEraResonanceDataRequest request, CancellationToken cancellationToken);
    Task<GetEraDataReferencesResponse> GetDataReferencesAsync(GetEraDataReferencesRequest request, CancellationToken cancellationToken);
}

public sealed partial class EraAbilityCatalogQueryService(IEraConfigurationStateProvider states,
    IEraLatticeDataProvider lattices, IEraResonanceDataProvider skills, IEraEffectDataProvider effects,
    IEraEnemyDataProvider enemies, IEraCombatRelationDataProvider relations, ILocalizationService localization)
    : EraConfigurationServiceBase(localization), IEraAbilityCatalogQueryService
{
    public async Task<EraDataView?> ReadAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var state = await states.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (state == null) return null;
            var relation = await relations.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (relation == null) return null;
            var records = new List<IEraDataDefinition>();
            // Repositories share one scoped DbContext, so reads must remain sequential.
            records.AddRange(await lattices.GetDefinitionsAsync(cancellationToken).ConfigureAwait(false));
            records.AddRange(await skills.GetDefinitionsAsync(cancellationToken).ConfigureAwait(false));
            records.AddRange(await effects.GetDefinitionsAsync(cancellationToken).ConfigureAwait(false));
            records.AddRange(await enemies.GetDefinitionsAsync(cancellationToken).ConfigureAwait(false));
            var after = await states.GetAsync(false, cancellationToken).ConfigureAwait(false);
            if (after?.Revision != state.Revision) continue;
            state.SettingsPayload = JsonSerializer.Serialize(new EraDataSettings
            {
                CombatRelations = JsonSerializer.Deserialize<CombatRelationConfigurationDto>(relation.Payload)
            });
            return new(state, records);
        }
        throw new InvalidOperationException("配置读取期间发生变化，请重试。");
    }

    public async Task<ResonanceConfiguration?> ReadConfigurationAsync(CancellationToken cancellationToken)
    {
        var view = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return view == null ? null : EraDataCatalog.Compile(view);
    }

    public async Task<GetEraResonanceDataResponse> GetDataStatusAsync(GetEraResonanceDataRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return view == null ? DataUnavailable<GetEraResonanceDataResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = new() { Revision = view.Workspace.Revision, SettingsVersion = view.Workspace.SettingsVersion, Settings = JsonSerializer.Deserialize<EraDataSettings>(view.Workspace.SettingsPayload)! }
        };
    }

    public async Task<ExportEraResonanceDataResponse> ExportDataAsync(ExportEraResonanceDataRequest request, CancellationToken cancellationToken)
    {
        var view = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return view == null ? DataUnavailable<ExportEraResonanceDataResponse>() : new()
        {
            Code = HttpStatusCode.OK,
            Data = EraDataCatalog.Compile(view)
        };
    }

    public async Task<GetEraResonanceConfigurationResponse> GetConfigurationAsync(
        GetEraResonanceConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var configurationRecord = await ReadConfigurationAsync(cancellationToken)
            .ConfigureAwait(false);

        if (configurationRecord == null)
        {
            return Error<GetEraResonanceConfigurationResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.skills_config_unavailable",
                text => text.ConfigUnavailable);
        }

        var configuration = configurationRecord;
        if (configuration.SchemaVersion != 3)
            return Error<GetEraResonanceConfigurationResponse>(HttpStatusCode.ServiceUnavailable, "era.skills_config_incompatible", text => text.ConfigUnavailable);
        ResonanceConfigurationValidator.Validate(configuration);
        configuration.Enemies = null;
        return new GetEraResonanceConfigurationResponse()
        {
            Code = HttpStatusCode.OK,
            Data = configuration
        };
    }

}
