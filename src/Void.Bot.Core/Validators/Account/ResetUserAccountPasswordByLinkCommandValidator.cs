using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;
namespace Void.Bot.Core.Validators.Account;
public sealed class ResetUserAccountPasswordByLinkCommandValidator
    : FluentMessageValidator<ResetUserAccountPasswordByLinkCommand>
{
    public ResetUserAccountPasswordByLinkCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().Length(43);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
        RuleFor(x => x.NewPasswordConfirm).Equal(x => x.NewPassword);
    }
}
