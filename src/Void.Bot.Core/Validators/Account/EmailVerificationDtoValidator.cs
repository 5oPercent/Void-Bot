using FluentValidation;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Core.Validators.Account;

public sealed class EmailVerificationDtoValidator : AbstractValidator<EmailVerificationDto>
{
    public EmailVerificationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters");

        RuleFor(x => x.ChallengeId)
            .NotEmpty().WithMessage("Email verification challenge is required");

        RuleFor(x => x.Code)
            .Matches(@"^\d{6}$").WithMessage("Email verification code must contain 6 digits");
    }
}
