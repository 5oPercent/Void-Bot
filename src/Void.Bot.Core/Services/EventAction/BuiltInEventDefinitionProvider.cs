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

public class AccountPasswordForceResetEventDefinitionProvider : IBuiltInEventDefinitionProvider
{
    public BuiltInEventDefinitionDto Definition => new()
    {
        Name = BuiltInEventNames.AccountPasswordForceReset,
        DisplayName = "账号密码被强制重置",
        Description = "管理人员强制重置指定账号密码且事务提交成功后执行配置动作。",
        Fields =
        [
            new BuiltInEventFieldDefinitionDto
            {
                Name = "UserId",
                DisplayName = "用户 ID",
                Description = "密码被强制重置的 Void 用户 ID。",
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
        if (builtInEvent is not AccountPasswordForceResetEvent passwordForceReset)
        {
            throw new InvalidOperationException(
                $"Event definition provider '{Definition.Name}' does not support event '{builtInEvent.Name}'.");
        }

        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["UserId"] = [passwordForceReset.UserId.ToString()]
        };
    }
}

public class AccountThirdPartyForceUnlinkedEventDefinitionProvider : IBuiltInEventDefinitionProvider
{
    public BuiltInEventDefinitionDto Definition => new()
    {
        Name = BuiltInEventNames.AccountThirdPartyForceUnlinked,
        DisplayName = "账号第三方授权被强制解绑",
        Description = "管理人员强制解绑指定账号的第三方授权且事务提交成功后执行配置动作。",
        Fields =
        [
            new BuiltInEventFieldDefinitionDto
            {
                Name = "UserId",
                DisplayName = "用户 ID",
                Description = "第三方授权被强制解绑的 Void 用户 ID。",
                ValueType = "Guid",
                Operators =
                [
                    EventActionConditionOperators.Equal,
                    EventActionConditionOperators.NotEqual,
                    EventActionConditionOperators.In,
                    EventActionConditionOperators.NotIn
                ]
            },
            new BuiltInEventFieldDefinitionDto
            {
                Name = "Platform",
                DisplayName = "第三方平台",
                Description = "本次被强制解绑的第三方授权平台。",
                ValueType = "String",
                Operators =
                [
                    EventActionConditionOperators.Equal,
                    EventActionConditionOperators.NotEqual,
                    EventActionConditionOperators.In,
                    EventActionConditionOperators.NotIn
                ],
                Options =
                [
                    new EventActionArgumentOptionDto { Value = "Steam", Label = "Steam" },
                    new EventActionArgumentOptionDto { Value = "Bungie", Label = "Bungie" },
                    new EventActionArgumentOptionDto { Value = "Discord", Label = "Discord" },
                    new EventActionArgumentOptionDto { Value = "Tencent", Label = "Tencent" }
                ]
            }
        ]
    };

    public IReadOnlyDictionary<string, string[]> GetFieldValues(IBuiltInEvent builtInEvent)
    {
        if (builtInEvent is not AccountThirdPartyForceUnlinkedEvent thirdPartyForceUnlinked)
        {
            throw new InvalidOperationException(
                $"Event definition provider '{Definition.Name}' does not support event '{builtInEvent.Name}'.");
        }

        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["UserId"] = [thirdPartyForceUnlinked.UserId.ToString()],
            ["Platform"] = [thirdPartyForceUnlinked.Platform.ToString()]
        };
    }
}
