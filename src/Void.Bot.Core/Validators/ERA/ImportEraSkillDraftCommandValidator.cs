using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>技能草稿管理命令参数校验。</summary>
public sealed class ImportEraSkillDraftCommandValidator : FluentMessageValidator<ImportEraSkillDraftCommand>
{
    public ImportEraSkillDraftCommandValidator()
    {
        RuleFor(c=>c.ExpectedDraftRevision).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.Mode).IsInEnum();
        RuleFor(c=>c.Configuration).NotNull().Must(Valid);
    }
    private static bool Valid(SkillConfiguration? value)
    {
        if(value==null)return false;
        try{SkillConfigurationValidator.Validate(value);return true;}catch(ArgumentException){return false;}
    }
}
