using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceService
{
    Task<GetEraResonanceProfileResponse> GetProfileAsync(
        GetEraResonanceProfileRequest request,
        CancellationToken cancellationToken);

    Task<SaveEraResonanceProfileResponse> SaveProfileAsync(
        SaveEraResonanceProfileCommand command,
        CancellationToken cancellationToken);
}

public partial class EraResonanceService
{
    public async Task<GetEraResonanceProfileResponse> GetProfileAsync(
        GetEraResonanceProfileRequest request,
        CancellationToken cancellationToken)
    {
        var character = await _characterContext.RequireOwnedAsync(request.CharacterId, false, cancellationToken).ConfigureAwait(false);

        var profileRecord = await _dataProvider
            .GetProfileAsync(character.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GetEraResonanceProfileResponse()
        {
            Code = HttpStatusCode.OK,
            Data = DecodeProfile(profileRecord)
        };
    }

    public async Task<SaveEraResonanceProfileResponse> SaveProfileAsync(
        SaveEraResonanceProfileCommand command,
        CancellationToken cancellationToken)
    {
        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);

        var profileRecord = await _dataProvider
            .GetProfileAsync(character.Id, cancellationToken)
            .ConfigureAwait(false);
        var currentVersion = profileRecord?.Version ?? 0;
        if (currentVersion != command.ExpectedVersion)
        {
            return Error<SaveEraResonanceProfileResponse>(
                HttpStatusCode.Conflict,
                "era.skills_version_conflict",
                text => text.VersionConflict);
        }

        var configurationRecord = await _dataProvider
            .GetConfigurationAsync(false, cancellationToken)
            .ConfigureAwait(false);
        if (configurationRecord == null)
        {
            return Error<SaveEraResonanceProfileResponse>(
                HttpStatusCode.ServiceUnavailable,
                "era.skills_config_unavailable",
                text => text.ConfigUnavailable);
        }

        try
        {
            ResonanceConfigurationValidator.ValidateProfile(
                DecodeConfiguration(configurationRecord),
                command.Profile);
        }
        catch (ArgumentException)
        {
            return Error<SaveEraResonanceProfileResponse>(
                HttpStatusCode.BadRequest,
                "era.skills_invalid_loadout",
                text => text.InvalidLoadout);
        }

        var payload = JsonSerializer.Serialize(command.Profile);
        var profile = JsonSerializer.Deserialize<CharacterResonanceProfile>(payload);
        if (profile == null)
        {
            throw new InvalidOperationException("Resonance profile payload is empty.");
        }

        profile.Version = checked(command.ExpectedVersion + 1);
        if (profileRecord == null)
        {
            profileRecord = new EraCharacterResonanceProfile()
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

        return new SaveEraResonanceProfileResponse()
        {
            Code = HttpStatusCode.OK,
            Data = profile
        };
    }

    private CharacterResonanceProfile DecodeProfile(EraCharacterResonanceProfile? profileRecord)
    {
        if (profileRecord == null)
        {
            return new CharacterResonanceProfile();
        }

        return _mapper.Map<CharacterResonanceProfile>(profileRecord);
    }
}
