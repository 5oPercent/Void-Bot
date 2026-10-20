using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class HeartbeatEraWorldCommandValidator : FluentMessageValidator<HeartbeatEraWorldCommand>
{
    public HeartbeatEraWorldCommandValidator()
    {
        RuleFor(command => command.WorldInstanceId).NotEmpty();
        RuleFor(command => command.MemberId).NotEmpty();
    }
}
