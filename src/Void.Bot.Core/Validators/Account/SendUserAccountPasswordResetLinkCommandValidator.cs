using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;
namespace Void.Bot.Core.Validators.Account;
public sealed class SendUserAccountPasswordResetLinkCommandValidator
    : FluentMessageValidator<SendUserAccountPasswordResetLinkCommand>
{
    public SendUserAccountPasswordResetLinkCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}
