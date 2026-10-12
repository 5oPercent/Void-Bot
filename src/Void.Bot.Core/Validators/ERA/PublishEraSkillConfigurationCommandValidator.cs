using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class PublishEraSkillConfigurationCommandValidator : FluentMessageValidator<PublishEraSkillConfigurationCommand>
{
    public PublishEraSkillConfigurationCommandValidator()
    {
        RuleFor(command => command.ExpectedVersion)
            .InclusiveBetween(0, long.MaxValue - 1);
        RuleFor(command => command.Configuration)
            .NotNull()
            .Must(HaveValidConfiguration);
    }

    private static bool HaveValidConfiguration(SkillConfiguration? configuration)
    {
        if (configuration == null)
        {
            return false;
        }

        try
        {
            SkillConfigurationValidator.Validate(configuration);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
