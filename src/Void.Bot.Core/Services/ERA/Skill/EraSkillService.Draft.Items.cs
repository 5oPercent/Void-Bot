using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

public partial interface IEraSkillService
{
    Task<GetEraSkillDraftResponse> GetDraftStatusAsync(GetEraSkillDraftRequest request, CancellationToken cancellationToken);
    Task<InitializeEraSkillDraftResponse> InitializeDraftAsync(InitializeEraSkillDraftCommand command, CancellationToken cancellationToken);
    Task<ExportEraSkillDraftResponse> ExportDraftAsync(ExportEraSkillDraftRequest request, CancellationToken cancellationToken);
    Task<ListEraDraftSystemResponse> ListDraftSystemAsync(ListEraDraftSystemRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftSystemResponse> GetDraftSystemAsync(GetEraDraftSystemRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftSystemResponse> SaveDraftSystemAsync(SaveEraDraftSystemCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDraftSystemResponse> DeleteDraftSystemAsync(DeleteEraDraftSystemCommand command, CancellationToken cancellationToken);
    Task<RestoreEraDraftSystemResponse> RestoreDraftSystemAsync(RestoreEraDraftSystemCommand command, CancellationToken cancellationToken);
    Task<ListEraDraftSkillResponse> ListDraftSkillAsync(ListEraDraftSkillRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftSkillResponse> GetDraftSkillAsync(GetEraDraftSkillRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftSkillResponse> SaveDraftSkillAsync(SaveEraDraftSkillCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDraftSkillResponse> DeleteDraftSkillAsync(DeleteEraDraftSkillCommand command, CancellationToken cancellationToken);
    Task<RestoreEraDraftSkillResponse> RestoreDraftSkillAsync(RestoreEraDraftSkillCommand command, CancellationToken cancellationToken);
    Task<ListEraDraftBuffResponse> ListDraftBuffAsync(ListEraDraftBuffRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftBuffResponse> GetDraftBuffAsync(GetEraDraftBuffRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftBuffResponse> SaveDraftBuffAsync(SaveEraDraftBuffCommand command, CancellationToken cancellationToken);
    Task<DeleteEraDraftBuffResponse> DeleteDraftBuffAsync(DeleteEraDraftBuffCommand command, CancellationToken cancellationToken);
    Task<RestoreEraDraftBuffResponse> RestoreDraftBuffAsync(RestoreEraDraftBuffCommand command, CancellationToken cancellationToken);
    Task<GetEraDraftSettingsResponse> GetDraftSettingsAsync(GetEraDraftSettingsRequest request, CancellationToken cancellationToken);
    Task<SaveEraDraftSettingsResponse> SaveDraftSettingsAsync(SaveEraDraftSettingsCommand command, CancellationToken cancellationToken);
}

public partial class EraSkillService
{
    public Task<ListEraDraftSystemResponse> ListDraftSystemAsync(ListEraDraftSystemRequest request, CancellationToken cancellationToken)
    {
        return ListDraftItemsAsync<ListEraDraftSystemResponse, AbilitySystemDefinition>(EraSkillDraftKind.System, request.Skip, request.Take, request.Search, request.IncludeDeleted, cancellationToken);
    }

    public Task<GetEraDraftSystemResponse> GetDraftSystemAsync(GetEraDraftSystemRequest request, CancellationToken cancellationToken)
    {
        return GetDraftItemAsync<GetEraDraftSystemResponse, AbilitySystemDefinition>(EraSkillDraftKind.System, request.Id, cancellationToken);
    }

    public Task<SaveEraDraftSystemResponse> SaveDraftSystemAsync(SaveEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        return SaveDraftItemAsync<SaveEraDraftSystemResponse, AbilitySystemDefinition>(EraSkillDraftKind.System, command.Id, command.ExpectedVersion, command.Definition, cancellationToken);
    }

    public Task<DeleteEraDraftSystemResponse> DeleteDraftSystemAsync(DeleteEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<DeleteEraDraftSystemResponse, AbilitySystemDefinition>(EraSkillDraftKind.System, command.Id, command.ExpectedVersion, true, cancellationToken);
    }

    public Task<RestoreEraDraftSystemResponse> RestoreDraftSystemAsync(RestoreEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<RestoreEraDraftSystemResponse, AbilitySystemDefinition>(EraSkillDraftKind.System, command.Id, command.ExpectedVersion, false, cancellationToken);
    }

    public Task<ListEraDraftSkillResponse> ListDraftSkillAsync(ListEraDraftSkillRequest request, CancellationToken cancellationToken)
    {
        return ListDraftItemsAsync<ListEraDraftSkillResponse, SkillDefinition>(EraSkillDraftKind.Skill, request.Skip, request.Take, request.Search, request.IncludeDeleted, cancellationToken);
    }

    public Task<GetEraDraftSkillResponse> GetDraftSkillAsync(GetEraDraftSkillRequest request, CancellationToken cancellationToken)
    {
        return GetDraftItemAsync<GetEraDraftSkillResponse, SkillDefinition>(EraSkillDraftKind.Skill, request.Id, cancellationToken);
    }

    public Task<SaveEraDraftSkillResponse> SaveDraftSkillAsync(SaveEraDraftSkillCommand command, CancellationToken cancellationToken)
    {
        return SaveDraftItemAsync<SaveEraDraftSkillResponse, SkillDefinition>(EraSkillDraftKind.Skill, command.Id, command.ExpectedVersion, command.Definition, cancellationToken);
    }

    public Task<DeleteEraDraftSkillResponse> DeleteDraftSkillAsync(DeleteEraDraftSkillCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<DeleteEraDraftSkillResponse, SkillDefinition>(EraSkillDraftKind.Skill, command.Id, command.ExpectedVersion, true, cancellationToken);
    }

    public Task<RestoreEraDraftSkillResponse> RestoreDraftSkillAsync(RestoreEraDraftSkillCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<RestoreEraDraftSkillResponse, SkillDefinition>(EraSkillDraftKind.Skill, command.Id, command.ExpectedVersion, false, cancellationToken);
    }

    public Task<ListEraDraftBuffResponse> ListDraftBuffAsync(ListEraDraftBuffRequest request, CancellationToken cancellationToken)
    {
        return ListDraftItemsAsync<ListEraDraftBuffResponse, BuffDefinitionDto>(EraSkillDraftKind.Buff, request.Skip, request.Take, request.Search, request.IncludeDeleted, cancellationToken);
    }

    public Task<GetEraDraftBuffResponse> GetDraftBuffAsync(GetEraDraftBuffRequest request, CancellationToken cancellationToken)
    {
        return GetDraftItemAsync<GetEraDraftBuffResponse, BuffDefinitionDto>(EraSkillDraftKind.Buff, request.Id, cancellationToken);
    }

    public Task<SaveEraDraftBuffResponse> SaveDraftBuffAsync(SaveEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        return SaveDraftItemAsync<SaveEraDraftBuffResponse, BuffDefinitionDto>(EraSkillDraftKind.Buff, command.Id, command.ExpectedVersion, command.Definition, cancellationToken);
    }

    public Task<DeleteEraDraftBuffResponse> DeleteDraftBuffAsync(DeleteEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<DeleteEraDraftBuffResponse, BuffDefinitionDto>(EraSkillDraftKind.Buff, command.Id, command.ExpectedVersion, true, cancellationToken);
    }

    public Task<RestoreEraDraftBuffResponse> RestoreDraftBuffAsync(RestoreEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        return ChangeDraftDeletionAsync<RestoreEraDraftBuffResponse, BuffDefinitionDto>(EraSkillDraftKind.Buff, command.Id, command.ExpectedVersion, false, cancellationToken);
    }
}
