using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Effect;

public interface IEraEffectService : IScope
{
    Task<ListEraDataBuffResponse> ListDataBuffAsync(ListEraDataBuffRequest request, CancellationToken cancellationToken);
    Task<GetEraDataBuffResponse> GetDataBuffAsync(GetEraDataBuffRequest request, CancellationToken cancellationToken);
    Task<SaveEraDataBuffResponse> SaveDataBuffAsync(SaveEraDataBuffCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDataBuffResponse> DeleteDataBuffAsync(DeleteEraDataBuffCommand command, CancellationToken cancellationToken);
}

/// <summary>本类别配置的独立业务入口，事务由 Mediator 工作单元提交。</summary>
public sealed class EraEffectService(IEraEffectDataProvider records, IEraAbilityCatalogQueryService catalog,
    IEraConfigurationStateProvider states, ICurrentUser currentUser, ILocalizationService localization)
    : EraDefinitionService<EraEffectDefinition, BuffDefinitionDto>(records, catalog, states, currentUser, localization), IEraEffectService
{
    protected override EraDataKind Kind => EraDataKind.Buff;

    protected override void SetDefinition(EraEffectDefinition record, BuffDefinitionDto definition)
    {
        base.SetDefinition(record, definition);
        record.IsDisabled = definition.IsDisabled;
    }

    public Task<ListEraDataBuffResponse> ListDataBuffAsync(ListEraDataBuffRequest request, CancellationToken cancellationToken) =>
        ListAsync<ListEraDataBuffResponse>(request.Skip, request.Take, request.Search, cancellationToken);

    public Task<GetEraDataBuffResponse> GetDataBuffAsync(GetEraDataBuffRequest request, CancellationToken cancellationToken) =>
        GetAsync<GetEraDataBuffResponse>(request.Id, cancellationToken);

    public Task<SaveEraDataBuffResponse> SaveDataBuffAsync(SaveEraDataBuffCommand command, CancellationToken cancellationToken) =>
        SaveAsync<SaveEraDataBuffResponse>(command.Id, command.ExpectedVersion, command.Definition, cancellationToken);

    public Task<DeleteEraDataBuffResponse> DeleteDataBuffAsync(DeleteEraDataBuffCommand command, CancellationToken cancellationToken) =>
        DeleteAsync<DeleteEraDataBuffResponse>(command.Id, command.ExpectedVersion, cancellationToken);
}
