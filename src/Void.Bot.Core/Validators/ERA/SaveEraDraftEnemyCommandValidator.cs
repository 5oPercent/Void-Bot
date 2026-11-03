using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>敌人配置草稿管理命令参数校验。</summary>
public sealed class SaveEraDraftEnemyCommandValidator : FluentMessageValidator<SaveEraDraftEnemyCommand>
{
    public SaveEraDraftEnemyCommandValidator()
    {
        RuleFor(c=>c.Id).NotEmpty().MaximumLength(128);
        RuleFor(c=>c.ExpectedVersion).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.Definition).NotNull().Must(value=>Valid(value,EraResonanceDraftKind.Enemy));
    }
    private static bool Valid(object? value,EraResonanceDraftKind kind)
    {
        if(value==null)return false;
        try{ResonanceDraftDefinitionValidator.Definition(kind,value);return true;}catch(ArgumentException){return false;}
    }
}
