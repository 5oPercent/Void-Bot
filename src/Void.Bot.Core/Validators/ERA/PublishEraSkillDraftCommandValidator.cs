using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;
/// <summary>技能草稿管理命令参数校验。</summary>
public sealed class PublishEraSkillDraftCommandValidator : FluentMessageValidator<PublishEraSkillDraftCommand>
{
    public PublishEraSkillDraftCommandValidator()
    {
        RuleFor(c=>c.ExpectedDraftRevision).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.ExpectedPublishedVersion).InclusiveBetween(0,long.MaxValue-1);
        RuleFor(c=>c.RequestId).NotEmpty();
    }

}
