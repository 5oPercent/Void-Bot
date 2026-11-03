using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceService
{
    Task<GetEraResonanceConfigurationResponse> GetConfigurationAsync(
        GetEraResonanceConfigurationRequest request,
        CancellationToken cancellationToken);


}

public partial class EraResonanceService
{
    public async Task<GetEraResonanceConfigurationResponse> GetConfigurationAsync(
        GetEraResonanceConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var configurationRecord = await _dataProvider
            .GetConfigurationAsync(false, cancellationToken)
            .ConfigureAwait(false);

        if (configurationRecord == null)
        {
            return Error<GetEraResonanceConfigurationResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.skills_config_unavailable",
                text => text.ConfigUnavailable);
        }

        var configuration = DecodeConfiguration(configurationRecord);
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

    private ResonanceConfiguration DecodeConfiguration(EraResonanceConfiguration configurationRecord)
    {
        var configuration = _mapper.Map<ResonanceConfiguration>(configurationRecord);
        ResonanceConfigurationValidator.Validate(configuration);
        return configuration;
    }
}
