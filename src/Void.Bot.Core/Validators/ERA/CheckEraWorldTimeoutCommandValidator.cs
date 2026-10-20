using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class CheckEraWorldTimeoutCommandValidator : FluentMessageValidator<CheckEraWorldTimeoutCommand>
{
    public CheckEraWorldTimeoutCommandValidator()
    {
        RuleFor(command => command.WorldInstanceId).NotEmpty();
    }
}
