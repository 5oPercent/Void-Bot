using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceService
{
    Task<GetEraResonanceDraftResponse> GetDraftStatusAsync(GetEraResonanceDraftRequest request, CancellationToken cancellationToken);
    Task<InitializeEraResonanceDraftResponse> InitializeDraftAsync(InitializeEraResonanceDraftCommand command, CancellationToken cancellationToken);
    Task<ExportEraResonanceDraftResponse> ExportDraftAsync(ExportEraResonanceDraftRequest request, CancellationToken cancellationToken);
    Task<ListEraDraftLatticeResponse> ListDraftLatticeAsync(ListEraDraftLatticeRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftLatticeResponse> GetDraftLatticeAsync(GetEraDraftLatticeRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftLatticeResponse> SaveDraftLatticeAsync(SaveEraDraftLatticeCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDraftLatticeResponse> DeleteDraftLatticeAsync(DeleteEraDraftLatticeCommand command, CancellationToken cancellationToken);
    Task<RestoreEraDraftLatticeResponse> RestoreDraftLatticeAsync(RestoreEraDraftLatticeCommand command, CancellationToken cancellationToken);
    Task<ListEraDraftResonanceResponse> ListDraftResonanceAsync(ListEraDraftResonanceRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftResonanceResponse> GetDraftResonanceAsync(GetEraDraftResonanceRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftResonanceResponse> SaveDraftResonanceAsync(SaveEraDraftResonanceCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDraftResonanceResponse> DeleteDraftResonanceAsync(DeleteEraDraftResonanceCommand command, CancellationToken cancellationToken);
    Task<RestoreEraDraftResonanceResponse> RestoreDraftResonanceAsync(RestoreEraDraftResonanceCommand command, CancellationToken cancellationToken);
    Task<ListEraDraftBuffResponse> ListDraftBuffAsync(ListEraDraftBuffRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftBuffResponse> GetDraftBuffAsync(GetEraDraftBuffRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftBuffResponse> SaveDraftBuffAsync(SaveEraDraftBuffCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDraftBuffResponse> DeleteDraftBuffAsync(DeleteEraDraftBuffCommand command, CancellationToken cancellationToken);
    Task<RestoreEraDraftBuffResponse> RestoreDraftBuffAsync(RestoreEraDraftBuffCommand command, CancellationToken cancellationToken);
    Task<ListEraDraftEnemyResponse> ListDraftEnemyAsync(ListEraDraftEnemyRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftEnemyResponse> GetDraftEnemyAsync(GetEraDraftEnemyRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftEnemyResponse> SaveDraftEnemyAsync(SaveEraDraftEnemyCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDraftEnemyResponse> DeleteDraftEnemyAsync(DeleteEraDraftEnemyCommand command, CancellationToken cancellationToken);
    Task<RestoreEraDraftEnemyResponse> RestoreDraftEnemyAsync(RestoreEraDraftEnemyCommand command, CancellationToken cancellationToken);
    Task<GetEraDraftSettingsResponse> GetDraftSettingsAsync(GetEraDraftSettingsRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftSettingsResponse> SaveDraftSettingsAsync(SaveEraDraftSettingsCommand command, CancellationToken cancellationToken);
}

public partial class EraResonanceService
{
    public Task<ListEraDraftLatticeResponse> ListDraftLatticeAsync(ListEraDraftLatticeRequest request, CancellationToken cancellationToken)
    {
        return ListDraftItemsAsync<ListEraDraftLatticeResponse, LatticeDefinition>(EraResonanceDraftKind.Lattice, request.Skip, request.Take, request.Search, request.IncludeDeleted, cancellationToken);
    }

    public Task<GetEraDraftLatticeResponse> GetDraftLatticeAsync(GetEraDraftLatticeRequest request, CancellationToken cancellationToken)
    {
        return GetDraftItemAsync<GetEraDraftLatticeResponse, LatticeDefinition>(EraResonanceDraftKind.Lattice, request.Id, cancellationToken);
    }

    public Task<SaveEraDraftLatticeResponse> SaveDraftLatticeAsync(SaveEraDraftLatticeCommand command, CancellationToken cancellationToken)
    {
        return SaveDraftItemAsync<SaveEraDraftLatticeResponse, LatticeDefinition>(EraResonanceDraftKind.Lattice, command.Id, command.ExpectedVersion, command.Definition, cancellationToken);
    }

    public Task<DeleteEraDraftLatticeResponse> DeleteDraftLatticeAsync(DeleteEraDraftLatticeCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<DeleteEraDraftLatticeResponse, LatticeDefinition>(EraResonanceDraftKind.Lattice, command.Id, command.ExpectedVersion, true, cancellationToken);
    }

    public Task<RestoreEraDraftLatticeResponse> RestoreDraftLatticeAsync(RestoreEraDraftLatticeCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<RestoreEraDraftLatticeResponse, LatticeDefinition>(EraResonanceDraftKind.Lattice, command.Id, command.ExpectedVersion, false, cancellationToken);
    }

    public Task<ListEraDraftResonanceResponse> ListDraftResonanceAsync(ListEraDraftResonanceRequest request, CancellationToken cancellationToken)
    {
        return ListDraftItemsAsync<ListEraDraftResonanceResponse, ResonanceDefinition>(EraResonanceDraftKind.Resonance, request.Skip, request.Take, request.Search, request.IncludeDeleted, cancellationToken);
    }

    public Task<GetEraDraftResonanceResponse> GetDraftResonanceAsync(GetEraDraftResonanceRequest request, CancellationToken cancellationToken)
    {
        return GetDraftItemAsync<GetEraDraftResonanceResponse, ResonanceDefinition>(EraResonanceDraftKind.Resonance, request.Id, cancellationToken);
    }

    public Task<SaveEraDraftResonanceResponse> SaveDraftResonanceAsync(SaveEraDraftResonanceCommand command, CancellationToken cancellationToken)
    {
        return SaveDraftItemAsync<SaveEraDraftResonanceResponse, ResonanceDefinition>(EraResonanceDraftKind.Resonance, command.Id, command.ExpectedVersion, command.Definition, cancellationToken);
    }

    public Task<DeleteEraDraftResonanceResponse> DeleteDraftResonanceAsync(DeleteEraDraftResonanceCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<DeleteEraDraftResonanceResponse, ResonanceDefinition>(EraResonanceDraftKind.Resonance, command.Id, command.ExpectedVersion, true, cancellationToken);
    }

    public Task<RestoreEraDraftResonanceResponse> RestoreDraftResonanceAsync(RestoreEraDraftResonanceCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<RestoreEraDraftResonanceResponse, ResonanceDefinition>(EraResonanceDraftKind.Resonance, command.Id, command.ExpectedVersion, false, cancellationToken);
    }

    public Task<ListEraDraftBuffResponse> ListDraftBuffAsync(ListEraDraftBuffRequest request, CancellationToken cancellationToken)
    {
        return ListDraftItemsAsync<ListEraDraftBuffResponse, BuffDefinitionDto>(EraResonanceDraftKind.Buff, request.Skip, request.Take, request.Search, request.IncludeDeleted, cancellationToken);
    }

    public Task<GetEraDraftBuffResponse> GetDraftBuffAsync(GetEraDraftBuffRequest request, CancellationToken cancellationToken)
    {
        return GetDraftItemAsync<GetEraDraftBuffResponse, BuffDefinitionDto>(EraResonanceDraftKind.Buff, request.Id, cancellationToken);
    }

    public Task<SaveEraDraftBuffResponse> SaveDraftBuffAsync(SaveEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        return SaveDraftItemAsync<SaveEraDraftBuffResponse, BuffDefinitionDto>(EraResonanceDraftKind.Buff, command.Id, command.ExpectedVersion, command.Definition, cancellationToken);
    }

    public Task<DeleteEraDraftBuffResponse> DeleteDraftBuffAsync(DeleteEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<DeleteEraDraftBuffResponse, BuffDefinitionDto>(EraResonanceDraftKind.Buff, command.Id, command.ExpectedVersion, true, cancellationToken);
    }

    public Task<RestoreEraDraftBuffResponse> RestoreDraftBuffAsync(RestoreEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<RestoreEraDraftBuffResponse, BuffDefinitionDto>(EraResonanceDraftKind.Buff, command.Id, command.ExpectedVersion, false, cancellationToken);
    }

    public Task<ListEraDraftEnemyResponse> ListDraftEnemyAsync(ListEraDraftEnemyRequest request, CancellationToken cancellationToken)
    {
        return ListDraftItemsAsync<ListEraDraftEnemyResponse, EnemyDefinition>(EraResonanceDraftKind.Enemy, request.Skip, request.Take, request.Search, request.IncludeDeleted, cancellationToken);
    }

    public Task<GetEraDraftEnemyResponse> GetDraftEnemyAsync(GetEraDraftEnemyRequest request, CancellationToken cancellationToken)
    {
        return GetDraftItemAsync<GetEraDraftEnemyResponse, EnemyDefinition>(EraResonanceDraftKind.Enemy, request.Id, cancellationToken);
    }

    public Task<SaveEraDraftEnemyResponse> SaveDraftEnemyAsync(SaveEraDraftEnemyCommand command, CancellationToken cancellationToken)
    {
        return SaveDraftItemAsync<SaveEraDraftEnemyResponse, EnemyDefinition>(EraResonanceDraftKind.Enemy, command.Id, command.ExpectedVersion, command.Definition, cancellationToken);
    }

    public Task<DeleteEraDraftEnemyResponse> DeleteDraftEnemyAsync(DeleteEraDraftEnemyCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<DeleteEraDraftEnemyResponse, EnemyDefinition>(EraResonanceDraftKind.Enemy, command.Id, command.ExpectedVersion, true, cancellationToken);
    }

    public Task<RestoreEraDraftEnemyResponse> RestoreDraftEnemyAsync(RestoreEraDraftEnemyCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<RestoreEraDraftEnemyResponse, EnemyDefinition>(EraResonanceDraftKind.Enemy, command.Id, command.ExpectedVersion, false, cancellationToken);
    }
}
