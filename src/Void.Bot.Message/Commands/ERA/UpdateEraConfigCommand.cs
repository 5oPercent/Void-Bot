using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class UpdateEraConfigCommand : ICommand
{
    /// <summary>读取配置时取得的版本，避免覆盖其他管理员的更新。</summary>
    public long ExpectedVersion { get; set; }
    public EraConfigDto Config { get; set; } = new EraConfigDto();
}

public class UpdateEraConfigResponse : EraResponse<EraConfigDto>
{
}
