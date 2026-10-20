using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

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
        var character = await _characterContext.RequireOwnedAsync(request.CharacterId, false, cancellationToken).ConfigureAwait(false);

        var profileRecord = await _dataProvider
            .GetProfileAsync(character.Id, cancellationToken)
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
        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);

        var profileRecord = await _dataProvider
            .GetProfileAsync(character.Id, cancellationToken)
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
            .GetLearnedSkillsAsync(character.Id, cancellationToken)
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
        var profile = JsonSerializer.Deserialize<CharacterSkillProfile>(payload);
        if (profile == null)
        {
            throw new InvalidOperationException("Skill profile payload is empty.");
        }

        profile.Version = checked(command.ExpectedVersion + 1);
        if (profileRecord == null)
        {
            profileRecord = new EraCharacterSkillProfile()
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
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

    private CharacterSkillProfile DecodeProfile(EraCharacterSkillProfile? profileRecord)
    {
        if (profileRecord == null)
        {
            return new CharacterSkillProfile();
        }

        return _mapper.Map<CharacterSkillProfile>(profileRecord);
    }
}
