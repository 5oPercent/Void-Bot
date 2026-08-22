using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public interface IBuiltInEventDefinitionProvider : IScope
{
    BuiltInEventDefinitionDto Definition { get; }
}

public class AccountRegisteredEventDefinitionProvider : IBuiltInEventDefinitionProvider
{
    public BuiltInEventDefinitionDto Definition => new()
    {
        Name = BuiltInEventNames.AccountRegistered,
        DisplayName = "账号注册成功",
        Description = "Void 账号事务提交成功后执行配置动作。"
    };
}
