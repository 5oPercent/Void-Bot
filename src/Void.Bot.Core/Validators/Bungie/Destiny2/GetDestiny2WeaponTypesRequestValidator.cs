using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Validators.Bungie.Destiny2;

public class GetDestiny2WeaponTypesRequestValidator : FluentMessageValidator<GetDestiny2WeaponTypesRequest>
{
    public GetDestiny2WeaponTypesRequestValidator(Destiny2Setting setting)
    {
        RuleFor(x => x.Language)
            .Must(language => Destiny2WeaponLanguageValidation.IsSupported(language, setting))
            .WithMessage($"Language must be one of: {string.Join(", ", setting.SupportedLanguages)}");
    }
}
