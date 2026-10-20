using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

public partial interface IEraSkillService
{
    Task<GetEraSkillConfigurationResponse> GetConfigurationAsync(
        GetEraSkillConfigurationRequest request,
        CancellationToken cancellationToken);


}

public partial class EraSkillService
{
    public async Task<GetEraSkillConfigurationResponse> GetConfigurationAsync(
        GetEraSkillConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var configurationRecord = await _dataProvider
            .GetConfigurationAsync(false, cancellationToken)
            .ConfigureAwait(false);

        if (configurationRecord == null)
        {
            return Error<GetEraSkillConfigurationResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.skills_config_unavailable",
                text => text.ConfigUnavailable);
        }

        return new GetEraSkillConfigurationResponse()
        {
            Code = HttpStatusCode.OK,
            Data = DecodeConfiguration(configurationRecord)
        };
    }

    private SkillConfiguration DecodeConfiguration(EraSkillConfiguration configurationRecord)
    {
        var configuration = _mapper.Map<SkillConfiguration>(configurationRecord);
        SkillConfigurationValidator.Validate(configuration);
        return configuration;
    }
}
