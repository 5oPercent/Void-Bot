using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>谐振技能草稿管理命令参数校验。</summary>
public sealed class ImportEraResonanceDraftCommandValidator : FluentMessageValidator<ImportEraResonanceDraftCommand>
{
    public ImportEraResonanceDraftCommandValidator()
    {
        RuleFor(c=>c.ExpectedDraftRevision).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.Mode).IsInEnum();
        RuleFor(c=>c.Configuration).NotNull().Must(Valid);
    }
    private static bool Valid(ResonanceConfiguration? value)
    {
        if(value==null)return false;
        try{ResonanceConfigurationValidator.Validate(value);return true;}catch(ArgumentException){return false;}
    }
}
