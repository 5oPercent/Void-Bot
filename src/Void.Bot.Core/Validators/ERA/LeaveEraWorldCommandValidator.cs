using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class LeaveEraWorldCommandValidator : FluentMessageValidator<LeaveEraWorldCommand>
{
    public LeaveEraWorldCommandValidator()
    {
        RuleFor(command => command.WorldInstanceId).NotEmpty();
        RuleFor(command => command.MemberId).NotEmpty();
        RuleFor(command => command.AuthorityEpoch).GreaterThan(0);
        RuleFor(command => command.ExpectedWorldVersion).GreaterThan(0);
    }
}
