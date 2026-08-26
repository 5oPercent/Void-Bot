using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Validators.Account;

public sealed class ForceUnlinkUserAccountThirdPartyCommandValidator
    : FluentMessageValidator<ForceUnlinkUserAccountThirdPartyCommand>
{
    public ForceUnlinkUserAccountThirdPartyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Platform)
            .IsInEnum().WithMessage("Third-party platform is invalid")
            .NotEqual(AppPlatformEnum.Void)
            .WithMessage("Void is not a third-party platform");
    }
}
