using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceService
{
    Task<ValidateEraResonanceDraftResponse> ValidateDraftAsync(ValidateEraResonanceDraftRequest request, CancellationToken cancellationToken);
    Task<PreviewEraResonanceDraftResponse> PreviewDraftAsync(PreviewEraResonanceDraftRequest request, CancellationToken cancellationToken);
    Task<GetEraDraftReferencesResponse> GetDraftReferencesAsync(GetEraDraftReferencesRequest request, CancellationToken cancellationToken);
    Task<ImportEraResonanceDraftResponse> ImportDraftAsync(ImportEraResonanceDraftCommand command, CancellationToken cancellationToken);
    Task<PublishEraResonanceDraftResponse> PublishDraftAsync(PublishEraResonanceDraftCommand command, CancellationToken cancellationToken);
    Task<ListEraResonancePublicationsResponse> ListPublicationsAsync(ListEraResonancePublicationsRequest request, CancellationToken cancellationToken);
    Task<GetEraResonancePublicationResponse> GetPublicationAsync(GetEraResonancePublicationRequest request, CancellationToken cancellationToken);
    Task<RestoreEraDraftPublicationResponse> RestorePublicationToDraftAsync(RestoreEraDraftPublicationCommand command, CancellationToken cancellationToken);
}
