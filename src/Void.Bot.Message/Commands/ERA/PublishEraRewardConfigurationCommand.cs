using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>内部配置发布能力，无 HTTP 路由；管理入口接入时必须另行完成管理权限校验。</summary>
[TransactionIsolation(IsolationLevel.ReadCommitted)]
public sealed class PublishEraRewardConfigurationCommand : ICommand
{
    public long ExpectedVersion { get; set; }
    public EraRewardRules Configuration { get; set; } = new EraRewardRules();
}

public sealed class PublishEraRewardConfigurationResponse : EraResponse<long>
{
}
