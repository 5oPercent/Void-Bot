using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Validators.Security;

public class GetPermissionRequestValidator : FluentMessageValidator<GetPermissionRequest>
{
    public GetPermissionRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}