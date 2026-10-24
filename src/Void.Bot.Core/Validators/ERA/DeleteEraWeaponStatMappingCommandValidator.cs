using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class DeleteEraWeaponStatMappingCommandValidator : FluentMessageValidator<DeleteEraWeaponStatMappingCommand>
{
    public DeleteEraWeaponStatMappingCommandValidator()
    {
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0).LessThan(long.MaxValue);
        RuleFor(command => command.Id).NotEmpty();
    }
}
