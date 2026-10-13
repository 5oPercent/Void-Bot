using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>技能草稿管理命令参数校验。</summary>
public sealed class SaveEraDraftSettingsCommandValidator : FluentMessageValidator<SaveEraDraftSettingsCommand>
{
    public SaveEraDraftSettingsCommandValidator()
    {
        RuleFor(c=>c.ExpectedVersion).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.Settings).NotNull().Must(Valid);
    }
    private static bool Valid(EraSkillDraftSettings? value)
    {
        if(value==null)return false;
        try{SkillDraftDefinitionValidator.Ids(value.CommonTacticalSkills);SkillDraftDefinitionValidator.Ids(value.DisabledSkills);return true;}catch(ArgumentException){return false;}
    }
}
