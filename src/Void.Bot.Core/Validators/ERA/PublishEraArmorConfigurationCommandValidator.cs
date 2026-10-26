using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class PublishEraArmorConfigurationCommandValidator : FluentMessageValidator<PublishEraArmorConfigurationCommand>
{
    public PublishEraArmorConfigurationCommandValidator()
    {
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0).LessThan(long.MaxValue);
        RuleFor(command => command.Configuration).NotNull();
    }
}
