using Void.Bot.Message.Constants.EventAction;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public interface IBuiltInEventDefinitionProvider : IScope
{
    BuiltInEventDefinitionDto Definition { get; }

    IReadOnlyDictionary<string, string[]> GetFieldValues(IBuiltInEvent builtInEvent);
}

public class AccountRegisteredEventDefinitionProvider : IBuiltInEventDefinitionProvider
{
    public BuiltInEventDefinitionDto Definition => new()
    {
        Name = BuiltInEventNames.AccountRegistered,
        DisplayName = "账号注册成功",
        Description = "Void 账号事务提交成功后执行配置动作。",
        Fields =
        [
            new BuiltInEventFieldDefinitionDto
            {
                Name = "UserId",
                DisplayName = "用户 ID",
                Description = "本次注册成功的 Void 用户 ID。",
                ValueType = "Guid",
                Operators =
                [
                    EventActionConditionOperators.Equal,
                    EventActionConditionOperators.NotEqual,
                    EventActionConditionOperators.In,
                    EventActionConditionOperators.NotIn
                ]
            }
        ]
    };

    public IReadOnlyDictionary<string, string[]> GetFieldValues(IBuiltInEvent builtInEvent)
    {
        if (builtInEvent is not AccountRegisteredEvent accountRegistered)
        {
            throw new InvalidOperationException(
                $"Event definition provider '{Definition.Name}' does not support event '{builtInEvent.Name}'.");
        }

        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["UserId"] = [accountRegistered.UserId.ToString()]
        };
    }
}
