using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>谐振技能配置管理命令参数校验。</summary>
public sealed class SaveEraDataSettingsCommandValidator : FluentMessageValidator<SaveEraDataSettingsCommand>
{
    public SaveEraDataSettingsCommandValidator()
    {
        RuleFor(c=>c.ExpectedVersion).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.Settings).NotNull().Must(Valid);
    }
    private static bool Valid(EraDataSettings? value)
    {
        if(value==null)return false;
        try{CombatRelationConfigurationValidator.Validate(value.CombatRelations);return true;}catch(ArgumentException){return false;}
    }
}
