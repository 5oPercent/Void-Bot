using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Enemy;

public interface IEraEnemyService : IScope
{
    Task<ListEraDataEnemyResponse> ListDataEnemyAsync(ListEraDataEnemyRequest request, CancellationToken cancellationToken);
    Task<GetEraDataEnemyResponse> GetDataEnemyAsync(GetEraDataEnemyRequest request, CancellationToken cancellationToken);
    Task<SaveEraDataEnemyResponse> SaveDataEnemyAsync(SaveEraDataEnemyCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDataEnemyResponse> DeleteDataEnemyAsync(DeleteEraDataEnemyCommand command, CancellationToken cancellationToken);
}

/// <summary>本类别配置的独立业务入口，事务由 Mediator 工作单元提交。</summary>
public sealed class EraEnemyService(IEraEnemyDataProvider records, IEraAbilityCatalogQueryService catalog,
    IEraConfigurationStateProvider states, ICurrentUser currentUser, ILocalizationService localization)
    : EraDefinitionService<EraEnemyDefinition, EnemyDefinition>(records, catalog, states, currentUser, localization), IEraEnemyService
{
    protected override EraDataKind Kind => EraDataKind.Enemy;

    public Task<ListEraDataEnemyResponse> ListDataEnemyAsync(ListEraDataEnemyRequest request, CancellationToken cancellationToken) =>
        ListAsync<ListEraDataEnemyResponse>(request.Skip, request.Take, request.Search, cancellationToken);

    public Task<GetEraDataEnemyResponse> GetDataEnemyAsync(GetEraDataEnemyRequest request, CancellationToken cancellationToken) =>
        GetAsync<GetEraDataEnemyResponse>(request.Id, cancellationToken);

    public Task<SaveEraDataEnemyResponse> SaveDataEnemyAsync(SaveEraDataEnemyCommand command, CancellationToken cancellationToken) =>
        SaveAsync<SaveEraDataEnemyResponse>(command.Id, command.ExpectedVersion, command.Definition, cancellationToken);

    public Task<DeleteEraDataEnemyResponse> DeleteDataEnemyAsync(DeleteEraDataEnemyCommand command, CancellationToken cancellationToken) =>
        DeleteAsync<DeleteEraDataEnemyResponse>(command.Id, command.ExpectedVersion, cancellationToken);
}
