using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public class RegisterUserAccountCommandValidator : FluentMessageValidator<RegisterUserAccountCommand>
{
    public RegisterUserAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required")
            .Length(1, 64).WithMessage("Name must be between 1 and 64 characters long");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$").WithMessage("Password must contain uppercase letters, lowercase letters, and digits");
    }
}