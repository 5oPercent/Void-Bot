using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraSkillService
{
    Task<ValidateEraSkillDraftResponse> ValidateDraftAsync(ValidateEraSkillDraftRequest request, CancellationToken cancellationToken);
    Task<PreviewEraSkillDraftResponse> PreviewDraftAsync(PreviewEraSkillDraftRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftReferencesResponse> GetDraftReferencesAsync(GetEraDraftReferencesRequest request, CancellationToken cancellationToken);
    Task<ImportEraSkillDraftResponse> ImportDraftAsync(ImportEraSkillDraftCommand command, CancellationToken cancellationToken);
    Task<PublishEraSkillDraftResponse> PublishDraftAsync(PublishEraSkillDraftCommand command, CancellationToken cancellationToken);
    Task<ListEraSkillPublicationsResponse> ListPublicationsAsync(ListEraSkillPublicationsRequest request, CancellationToken cancellationToken);
    Task<GetEraSkillPublicationResponse> GetPublicationAsync(GetEraSkillPublicationRequest request, CancellationToken cancellationToken);
    Task<RestoreEraDraftPublicationResponse> RestorePublicationToDraftAsync(RestoreEraDraftPublicationCommand command, CancellationToken cancellationToken);
}
