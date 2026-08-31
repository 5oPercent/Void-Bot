using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Validators.Bungie.Destiny2;

public class GetDestiny2WeaponTypesRequestValidator : FluentMessageValidator<GetDestiny2WeaponTypesRequest>
{
    public GetDestiny2WeaponTypesRequestValidator()
    {
    }
}
