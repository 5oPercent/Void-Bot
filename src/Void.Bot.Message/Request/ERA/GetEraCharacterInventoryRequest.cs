using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>读取本人角色背包，可按目标槽位筛选未装备候选。</summary>
public sealed class GetEraCharacterInventoryRequest : IRequest, IRequireEraPlayer
{
    /// <summary>由角色路由覆盖的角色 ID。</summary>
    public Guid CharacterId { get; set; }
    /// <summary>可选目标槽位；为空时返回全部背包实例，计数始终包含所有分区。</summary>
    public EraArmorSlot? Slot { get; set; }
}

/// <summary>候选与完整已穿状态来自同一个快照，不能与其他版本的查询结果拼接。</summary>
public sealed class GetEraCharacterInventoryResponse : EraResponse<EraCharacterEquipmentStateDto>
{
}
