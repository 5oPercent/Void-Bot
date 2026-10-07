using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class AdjustEraWarehouseCapacityCommand : ICommand
{
    /// <summary>由 API 路由写入，客户端请求体不能改变目标玩家。</summary>
    public Guid PlayerId { get; set; }

    /// <summary>同一操作重试必须复用此 ID；已成功操作返回原始回执。</summary>
    public Guid RequestId { get; set; }

    public long ExpectedVersion { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class AdjustEraWarehouseCapacityResponse : EraResponse<EraCapacityReceiptDto>
{
}
