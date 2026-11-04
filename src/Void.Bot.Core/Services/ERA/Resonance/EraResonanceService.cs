using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public interface IEraResonanceService : IScope
{
    Task<ListEraDataResonanceResponse> ListDataResonanceAsync(ListEraDataResonanceRequest request, CancellationToken cancellationToken);
    Task<GetEraDataResonanceResponse> GetDataResonanceAsync(GetEraDataResonanceRequest request, CancellationToken cancellationToken);
    Task<SaveEraDataResonanceResponse> SaveDataResonanceAsync(SaveEraDataResonanceCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDataResonanceResponse> DeleteDataResonanceAsync(DeleteEraDataResonanceCommand command, CancellationToken cancellationToken);
}

/// <summary>本类别配置的独立业务入口，事务由 Mediator 工作单元提交。</summary>
public sealed class EraResonanceService(IEraResonanceDataProvider records, IEraAbilityCatalogQueryService catalog,
    IEraConfigurationStateProvider states, ICurrentUser currentUser, ILocalizationService localization)
    : EraDefinitionService<EraSkillDefinition, ResonanceDefinition>(records, catalog, states, currentUser, localization), IEraResonanceService
{
    protected override EraDataKind Kind => EraDataKind.Resonance;

    protected override void SetDefinition(EraSkillDefinition record, ResonanceDefinition definition)
    {
        base.SetDefinition(record, definition);
        record.IsDisabled = definition.IsDisabled;
    }

    public Task<ListEraDataResonanceResponse> ListDataResonanceAsync(ListEraDataResonanceRequest request, CancellationToken cancellationToken) =>
        ListAsync<ListEraDataResonanceResponse>(request.Skip, request.Take, request.Search, cancellationToken);

    public Task<GetEraDataResonanceResponse> GetDataResonanceAsync(GetEraDataResonanceRequest request, CancellationToken cancellationToken) =>
        GetAsync<GetEraDataResonanceResponse>(request.Id, cancellationToken);

    public Task<SaveEraDataResonanceResponse> SaveDataResonanceAsync(SaveEraDataResonanceCommand command, CancellationToken cancellationToken) =>
        SaveAsync<SaveEraDataResonanceResponse>(command.Id, command.ExpectedVersion, command.Definition, cancellationToken);

    public Task<DeleteEraDataResonanceResponse> DeleteDataResonanceAsync(DeleteEraDataResonanceCommand command, CancellationToken cancellationToken) =>
        DeleteAsync<DeleteEraDataResonanceResponse>(command.Id, command.ExpectedVersion, cancellationToken);
}
