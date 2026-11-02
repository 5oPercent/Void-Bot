using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceService
{
    Task<GetEraLearnedResonancesResponse> GetLearnedResonancesAsync(
        GetEraLearnedResonancesRequest request,
        CancellationToken cancellationToken);

    Task<GrantEraLearnedResonanceResponse> GrantResonanceAsync(
        GrantEraLearnedResonanceCommand command,
        CancellationToken cancellationToken);
}

public partial class EraResonanceService
{
    public async Task<GetEraLearnedResonancesResponse> GetLearnedResonancesAsync(
        GetEraLearnedResonancesRequest request,
        CancellationToken cancellationToken)
    {
        var character = await _characterContext.RequireOwnedAsync(request.CharacterId, false, cancellationToken).ConfigureAwait(false);

        var learnedSkills = await _dataProvider
            .GetLearnedResonancesAsync(character.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GetEraLearnedResonancesResponse()
        {
            Code = HttpStatusCode.OK,
            Data = learnedSkills
        };
    }

    public async Task<GrantEraLearnedResonanceResponse> GrantResonanceAsync(
        GrantEraLearnedResonanceCommand command,
        CancellationToken cancellationToken)
    {
        var character = await _characterContext
            .RequireTargetAsync(command.CharacterId, true, cancellationToken)
            .ConfigureAwait(false);

        var configurationRecord = await _dataProvider
            .GetConfigurationAsync(false, cancellationToken)
            .ConfigureAwait(false);
        if (configurationRecord == null || !DecodeConfiguration(configurationRecord).Skills.Any(skill => skill.Id == command.SkillId))
        {
            return Error<GrantEraLearnedResonanceResponse>(
                HttpStatusCode.NotFound,
                "era.skills_not_found",
                text => text.SkillNotFound);
        }

        var learnedSkills = await _dataProvider
            .GetLearnedResonancesAsync(character.Id, cancellationToken)
            .ConfigureAwait(false);
        if (!learnedSkills.Contains(command.SkillId, StringComparer.Ordinal))
        {
            var learnedSkill = new EraCharacterLearnedResonance()
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                SkillId = command.SkillId
            };

            await _dataProvider
                .GrantResonanceAsync(learnedSkill, cancellationToken)
                .ConfigureAwait(false);
            learnedSkills.Add(command.SkillId);
        }

        return new GrantEraLearnedResonanceResponse()
        {
            Code = HttpStatusCode.OK,
            Data = learnedSkills.OrderBy(skillId => skillId, StringComparer.Ordinal).ToList()
        };
    }
}
