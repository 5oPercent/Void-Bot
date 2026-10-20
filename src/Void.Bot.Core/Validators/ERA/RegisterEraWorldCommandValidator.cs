using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class RegisterEraWorldCommandValidator : FluentMessageValidator<RegisterEraWorldCommand>
{
    public RegisterEraWorldCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty();
        RuleFor(command => command.MemberId).NotEmpty();
        RuleFor(command => command.AuthorityEpoch).GreaterThan(0);
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.WorldDefinitionId).NotEmpty().MaximumLength(128);
    }
}
