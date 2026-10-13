using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class SaveEraSkillProfileCommandValidator : FluentMessageValidator<SaveEraSkillProfileCommand>
{
    public SaveEraSkillProfileCommandValidator()
    {
        RuleFor(command => command.CharacterId)
            .NotEmpty();
        RuleFor(command => command.ExpectedVersion)
            .InclusiveBetween(0, long.MaxValue - 1);
        RuleFor(command => command.Profile)
            .NotNull()
            .Must(HaveValidProfile);
    }

    private static bool HaveValidProfile(CharacterSkillProfile? profile)
    {
        if (profile == null || profile.Loadouts == null || profile.Loadouts.Count > 256)
        {
            return false;
        }

        foreach (var loadout in profile.Loadouts)
        {
            if (loadout == null ||
                string.IsNullOrWhiteSpace(loadout.SystemId) ||
                loadout.SystemId.Length > 128 ||
                loadout.Slots == null || loadout.Slots.Length != 4)
            {
                return false;
            }

            foreach (var skillId in loadout.Slots)
            {
                if (skillId != null && (string.IsNullOrWhiteSpace(skillId) || skillId.Length > 128))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
