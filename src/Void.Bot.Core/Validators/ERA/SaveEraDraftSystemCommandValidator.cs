using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>技能草稿管理命令参数校验。</summary>
public sealed class SaveEraDraftSystemCommandValidator : FluentMessageValidator<SaveEraDraftSystemCommand>
{
    public SaveEraDraftSystemCommandValidator()
    {
        RuleFor(c=>c.Id).NotEmpty().MaximumLength(128);
        RuleFor(c=>c.ExpectedVersion).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.Definition).NotNull().Must(value=>Valid(value,EraSkillDraftKind.System));
    }
    private static bool Valid(object? value,EraSkillDraftKind kind)
    {
        if(value==null)return false;
        try{SkillDraftDefinitionValidator.Definition(kind,value);return true;}catch(ArgumentException){return false;}
    }
}
