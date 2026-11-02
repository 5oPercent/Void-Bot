using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>谐振技能草稿管理命令参数校验。</summary>
public sealed class RestoreEraDraftResonanceCommandValidator : FluentMessageValidator<RestoreEraDraftResonanceCommand>
{
    public RestoreEraDraftResonanceCommandValidator()
    {
        RuleFor(c=>c.Id).NotEmpty().MaximumLength(128);
        RuleFor(c=>c.ExpectedVersion).InclusiveBetween(1,long.MaxValue-1);
    }

}
