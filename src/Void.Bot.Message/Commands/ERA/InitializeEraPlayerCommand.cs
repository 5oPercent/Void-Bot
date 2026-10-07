using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>账号身份取自认证上下文，重复初始化返回同一玩家。</summary>
public class InitializeEraPlayerCommand : ICommand
{
}

public class InitializeEraPlayerResponse : EraResponse<EraPlayerDto>
{
}
