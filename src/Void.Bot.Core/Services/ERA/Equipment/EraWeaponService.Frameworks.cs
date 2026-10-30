using System.Net;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public partial interface IEraWeaponService
{
    Task<GetEraWeaponFrameworksResponse> GetFrameworksAsync(GetEraWeaponFrameworksRequest request, CancellationToken cancellationToken);
    Task<GetEraWeaponFrameworkResponse> GetFrameworkAsync(GetEraWeaponFrameworkRequest request, CancellationToken cancellationToken);
    Task<SaveEraWeaponFrameworkResponse> SaveFrameworkAsync(SaveEraWeaponFrameworkCommand command, CancellationToken cancellationToken);
    Task<DisableEraWeaponFrameworkResponse> DisableFrameworkAsync(DisableEraWeaponFrameworkCommand command, CancellationToken cancellationToken);
    Task<DeleteEraWeaponFrameworkResponse> DeleteFrameworkAsync(DeleteEraWeaponFrameworkCommand command, CancellationToken cancellationToken);
}

public sealed partial class EraWeaponService
{
    public async Task<GetEraWeaponFrameworksResponse> GetFrameworksAsync(GetEraWeaponFrameworksRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        var items = manifest.Weapons.Frameworks.Where(row => (!request.IsEnabled.HasValue || row.IsEnabled == request.IsEnabled) &&
            (request.Category == null || string.Equals(row.Category, request.Category, StringComparison.Ordinal)) &&
            (request.PresentationDriverKey == null || row.PresentationDriverKey == request.PresentationDriverKey) &&
            (string.IsNullOrWhiteSpace(request.Search) || row.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                row.Id.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase))).ToList();
        return new GetEraWeaponFrameworksResponse { Code = HttpStatusCode.OK, Version = manifest.Version, Data = Page(items, request.PageIndex, request.PageSize) };
    }

    public async Task<GetEraWeaponFrameworkResponse> GetFrameworkAsync(GetEraWeaponFrameworkRequest request, CancellationToken cancellationToken)
    {
        var manifest = await ReadAsync(cancellationToken).ConfigureAwait(false);
        return new GetEraWeaponFrameworkResponse
        {
            Code = HttpStatusCode.OK, Version = manifest.Version,
            Data = manifest.Weapons.Frameworks.SingleOrDefault(row => row.Id == request.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound),
            TemplateIds = manifest.Weapons.Templates.Where(row => row.FrameworkId == request.Id).Select(row => row.Id).ToList()
        };
    }

    public Task<SaveEraWeaponFrameworkResponse> SaveFrameworkAsync(SaveEraWeaponFrameworkCommand command, CancellationToken cancellationToken) =>
        WriteAsync<SaveEraWeaponFrameworkResponse>(command.ExpectedManifestVersion, catalog =>
        {
            catalog.Frameworks.RemoveAll(row => row.Id == command.Framework.Id);
            catalog.Frameworks.Add(command.Framework);
            return catalog;
        }, cancellationToken, () => command.Framework.Id);

    public Task<DisableEraWeaponFrameworkResponse> DisableFrameworkAsync(DisableEraWeaponFrameworkCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DisableEraWeaponFrameworkResponse>(command.ExpectedManifestVersion, catalog =>
        {
            var framework = catalog.Frameworks.SingleOrDefault(row => row.Id == command.Id) ??
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            // Do not mutate the original catalog used for change detection and reference checks.
            var disabled = System.Text.Json.JsonSerializer.Deserialize<EraWeaponFrameworkDto>(System.Text.Json.JsonSerializer.Serialize(framework))!;
            disabled.IsEnabled = false;
            catalog.Frameworks[catalog.Frameworks.IndexOf(framework)] = disabled;
            return catalog;
        }, cancellationToken);

    public Task<DeleteEraWeaponFrameworkResponse> DeleteFrameworkAsync(DeleteEraWeaponFrameworkCommand command, CancellationToken cancellationToken) =>
        WriteAsync<DeleteEraWeaponFrameworkResponse>(command.ExpectedManifestVersion, catalog =>
        {
            if (!catalog.Frameworks.Any(row => row.Id == command.Id))
                throw Error(HttpStatusCode.NotFound, "era.weapon_not_found", value => value.NotFound);
            if (catalog.Templates.Any(row => row.FrameworkId == command.Id))
                throw Error(HttpStatusCode.Conflict, "era.weapon_reference_conflict", value => value.ReferenceConflict);
            catalog.Frameworks.RemoveAll(row => row.Id == command.Id);
            return catalog;
        }, cancellationToken);
}
