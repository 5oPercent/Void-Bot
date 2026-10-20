using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

public partial interface IEraSkillService
{
    Task<GetEraLearnedSkillsResponse> GetLearnedSkillsAsync(
        GetEraLearnedSkillsRequest request,
        CancellationToken cancellationToken);

    Task<GrantEraLearnedSkillResponse> GrantSkillAsync(
        GrantEraLearnedSkillCommand command,
        CancellationToken cancellationToken);
}

public partial class EraSkillService
{
    public async Task<GetEraLearnedSkillsResponse> GetLearnedSkillsAsync(
        GetEraLearnedSkillsRequest request,
        CancellationToken cancellationToken)
    {
        var character = await _characterContext.RequireOwnedAsync(request.CharacterId, false, cancellationToken).ConfigureAwait(false);

        var learnedSkills = await _dataProvider
            .GetLearnedSkillsAsync(character.Id, cancellationToken)
            .ConfigureAwait(false);

        return new GetEraLearnedSkillsResponse()
        {
            Code = HttpStatusCode.OK,
            Data = learnedSkills
        };
    }

    public async Task<GrantEraLearnedSkillResponse> GrantSkillAsync(
        GrantEraLearnedSkillCommand command,
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
            return Error<GrantEraLearnedSkillResponse>(
                HttpStatusCode.NotFound,
                "era.skills_not_found",
                text => text.SkillNotFound);
        }

        var learnedSkills = await _dataProvider
            .GetLearnedSkillsAsync(character.Id, cancellationToken)
            .ConfigureAwait(false);
        if (!learnedSkills.Contains(command.SkillId, StringComparer.Ordinal))
        {
            var learnedSkill = new EraCharacterLearnedSkill()
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                SkillId = command.SkillId
            };

            await _dataProvider
                .GrantSkillAsync(learnedSkill, cancellationToken)
                .ConfigureAwait(false);
            learnedSkills.Add(command.SkillId);
        }

        return new GrantEraLearnedSkillResponse()
        {
            Code = HttpStatusCode.OK,
            Data = learnedSkills.OrderBy(skillId => skillId, StringComparer.Ordinal).ToList()
        };
    }
}
