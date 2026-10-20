namespace Void.Bot.Message.Dto.ERA;

/// <summary>装备写操作的统一响应，成功以 Mediator 工作单元提交为准。</summary>
public class EraEquipmentOperationResponse : EraResponse<EraEquipmentOperationReceiptDto>
{
    /// <summary>是否返回已有成功回执；为 true 时客户端须按版本处理历史状态，必要时重新查询。</summary>
    public bool IsReplay { get; set; }
}
