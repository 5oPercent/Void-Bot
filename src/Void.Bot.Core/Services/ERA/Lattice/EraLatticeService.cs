using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Lattice;

public interface IEraLatticeService : IScope
{
    Task<ListEraDataLatticeResponse> ListDataLatticeAsync(ListEraDataLatticeRequest request, CancellationToken cancellationToken);
    Task<GetEraDataLatticeResponse> GetDataLatticeAsync(GetEraDataLatticeRequest request, CancellationToken cancellationToken);
    Task<SaveEraDataLatticeResponse> SaveDataLatticeAsync(SaveEraDataLatticeCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDataLatticeResponse> DeleteDataLatticeAsync(DeleteEraDataLatticeCommand command, CancellationToken cancellationToken);
}

/// <summary>本类别配置的独立业务入口，事务由 Mediator 工作单元提交。</summary>
public sealed class EraLatticeService(IEraLatticeDataProvider records, IEraAbilityCatalogQueryService catalog,
    IEraConfigurationStateProvider states, ICurrentUser currentUser, ILocalizationService localization)
    : EraDefinitionService<EraLatticeDefinition, LatticeDefinition>(records, catalog, states, currentUser, localization), IEraLatticeService
{
    protected override EraDataKind Kind => EraDataKind.Lattice;

    public Task<ListEraDataLatticeResponse> ListDataLatticeAsync(ListEraDataLatticeRequest request, CancellationToken cancellationToken) =>
        ListAsync<ListEraDataLatticeResponse>(request.Skip, request.Take, request.Search, cancellationToken);

    public Task<GetEraDataLatticeResponse> GetDataLatticeAsync(GetEraDataLatticeRequest request, CancellationToken cancellationToken) =>
        GetAsync<GetEraDataLatticeResponse>(request.Id, cancellationToken);

    public Task<SaveEraDataLatticeResponse> SaveDataLatticeAsync(SaveEraDataLatticeCommand command, CancellationToken cancellationToken) =>
        SaveAsync<SaveEraDataLatticeResponse>(command.Id, command.ExpectedVersion, command.Definition, cancellationToken);

    public Task<DeleteEraDataLatticeResponse> DeleteDataLatticeAsync(DeleteEraDataLatticeCommand command, CancellationToken cancellationToken) =>
        DeleteAsync<DeleteEraDataLatticeResponse>(command.Id, command.ExpectedVersion, cancellationToken);
}
