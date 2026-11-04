using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>谐振技能配置管理命令参数校验。</summary>
public sealed class SaveEraDataLatticeCommandValidator : FluentMessageValidator<SaveEraDataLatticeCommand>
{
    public SaveEraDataLatticeCommandValidator()
    {
        RuleFor(c=>c.Id).NotEmpty().MaximumLength(128);
        RuleFor(c=>c.ExpectedVersion).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.Definition).NotNull().Must(value=>Valid(value,EraDataKind.Lattice));
    }
    private static bool Valid(object? value,EraDataKind kind)
    {
        if(value==null)return false;
        try{EraDataDefinitionValidator.Definition(kind,value);return true;}catch(ArgumentException){return false;}
    }
}
