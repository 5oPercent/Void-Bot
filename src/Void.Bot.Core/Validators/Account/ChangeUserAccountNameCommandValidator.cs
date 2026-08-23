using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public class ChangeUserAccountNameCommandValidator
    : FluentMessageValidator<ChangeUserAccountNameCommand>
{
    public ChangeUserAccountNameCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Name is required")
            .Length(1, 64).WithMessage("Name must be between 1 and 64 characters long")
            .Must(name => name is not null && name.All(character => !char.IsWhiteSpace(character)))
            .WithMessage("Name cannot contain whitespace characters");
    }
}
