using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraSkillService
{
    Task<GetEraSkillConfigurationResponse> GetConfigurationAsync(
        GetEraSkillConfigurationRequest request,
        CancellationToken cancellationToken);

    Task<PublishEraSkillConfigurationResponse> PublishAsync(
        PublishEraSkillConfigurationCommand command,
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

    public async Task<PublishEraSkillConfigurationResponse> PublishAsync(
        PublishEraSkillConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        var configurationRecord = await _dataProvider
            .GetConfigurationAsync(true, cancellationToken)
            .ConfigureAwait(false);

        if (configurationRecord == null)
        {
            return Error<PublishEraSkillConfigurationResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.skills_config_unavailable",
                text => text.ConfigUnavailable);
        }

        if (configurationRecord.Version != command.ExpectedVersion)
        {
            return Error<PublishEraSkillConfigurationResponse>(
                HttpStatusCode.Conflict,
                "era.skills_version_conflict",
                text => text.VersionConflict);
        }

        var payload = JsonSerializer.Serialize(command.Configuration);
        var configuration = JsonSerializer.Deserialize<SkillConfiguration>(payload);
        if (configuration == null)
        {
            throw new InvalidOperationException("Skill configuration payload is empty.");
        }

        configuration.Version = checked(configurationRecord.Version + 1);
        SkillConfigurationValidator.Validate(configuration);

        configurationRecord.Version = configuration.Version;
        configurationRecord.Payload = JsonSerializer.Serialize(configuration);
        configurationRecord.LastModifiedBy = _currentUser.Id;

        return new PublishEraSkillConfigurationResponse()
        {
            Code = HttpStatusCode.OK,
            Data = configuration
        };
    }

    private SkillConfiguration DecodeConfiguration(EraSkillConfiguration configurationRecord)
    {
        var configuration = _mapper.Map<SkillConfiguration>(configurationRecord);
        SkillConfigurationValidator.Validate(configuration);
        return configuration;
    }
}
