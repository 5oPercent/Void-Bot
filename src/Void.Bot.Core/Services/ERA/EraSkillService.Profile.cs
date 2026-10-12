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
    Task<GetEraSkillProfileResponse> GetProfileAsync(
        GetEraSkillProfileRequest request,
        CancellationToken cancellationToken);

    Task<SaveEraSkillProfileResponse> SaveProfileAsync(
        SaveEraSkillProfileCommand command,
        CancellationToken cancellationToken);
}

public partial class EraSkillService
{
    public async Task<GetEraSkillProfileResponse> GetProfileAsync(
        GetEraSkillProfileRequest request,
        CancellationToken cancellationToken)
    {
        var player = await _playerContext.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);

        var profileRecord = await _dataProvider
            .GetProfileAsync(player.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GetEraSkillProfileResponse()
        {
            Code = HttpStatusCode.OK,
            Data = DecodeProfile(profileRecord)
        };
    }

    public async Task<SaveEraSkillProfileResponse> SaveProfileAsync(
        SaveEraSkillProfileCommand command,
        CancellationToken cancellationToken)
    {
        var player = await _playerContext.RequireCurrentAsync(true, cancellationToken).ConfigureAwait(false);

        var profileRecord = await _dataProvider
            .GetProfileAsync(player.Id, cancellationToken)
            .ConfigureAwait(false);
        var currentVersion = profileRecord?.Version ?? 0;
        if (currentVersion != command.ExpectedVersion)
        {
            return Error<SaveEraSkillProfileResponse>(
                HttpStatusCode.Conflict,
                "era.skills_version_conflict",
                text => text.VersionConflict);
        }

        var configurationRecord = await _dataProvider
            .GetConfigurationAsync(false, cancellationToken)
            .ConfigureAwait(false);
        if (configurationRecord == null)
        {
            return Error<SaveEraSkillProfileResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.skills_config_unavailable",
                text => text.ConfigUnavailable);
        }

        var learnedSkillIds = await _dataProvider
            .GetLearnedSkillsAsync(player.Id, cancellationToken)
            .ConfigureAwait(false);
        var learnedSkills = learnedSkillIds.ToHashSet(StringComparer.Ordinal);

        try
        {
            SkillConfigurationValidator.ValidateProfile(
                DecodeConfiguration(configurationRecord),
                command.Profile,
                learnedSkills);
        }
        catch (ArgumentException)
        {
            return Error<SaveEraSkillProfileResponse>(
                HttpStatusCode.BadRequest,
                "era.skills_invalid_loadout",
                text => text.InvalidLoadout);
        }

        var payload = JsonSerializer.Serialize(command.Profile);
        var profile = JsonSerializer.Deserialize<PlayerSkillProfile>(payload);
        if (profile == null)
        {
            throw new InvalidOperationException("Skill profile payload is empty.");
        }

        profile.Version = checked(command.ExpectedVersion + 1);
        if (profileRecord == null)
        {
            profileRecord = new EraPlayerSkillProfile()
            {
                Id = Guid.NewGuid(),
                PlayerId = player.Id,
                Version = profile.Version,
                Payload = JsonSerializer.Serialize(profile)
            };

            await _dataProvider
                .CreateProfileAsync(profileRecord, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            profileRecord.Version = profile.Version;
            profileRecord.Payload = JsonSerializer.Serialize(profile);
            _dataProvider.UpdateProfile(profileRecord);
        }

        return new SaveEraSkillProfileResponse()
        {
            Code = HttpStatusCode.OK,
            Data = profile
        };
    }

    private PlayerSkillProfile DecodeProfile(EraPlayerSkillProfile? profileRecord)
    {
        if (profileRecord == null)
        {
            return new PlayerSkillProfile();
        }

        return _mapper.Map<PlayerSkillProfile>(profileRecord);
    }
}
