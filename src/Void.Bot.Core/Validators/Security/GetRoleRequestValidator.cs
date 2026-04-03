using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Validators.Security;

public class GetRoleRequestValidator : FluentMessageValidator<GetRoleRequest>
{
    public GetRoleRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}