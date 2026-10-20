using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>读取本人角色的完整背包与已穿装备快照。</summary>
public sealed class GetEraCharacterEquipmentRequest : IRequest, IRequireEraPlayer
{
    /// <summary>由角色路由覆盖的角色 ID。</summary>
    public Guid CharacterId { get; set; }
}

/// <summary>一致的角色装备状态，不包含尚未接入的世界操作限制。</summary>
public sealed class GetEraCharacterEquipmentResponse : EraResponse<EraCharacterEquipmentStateDto>
{
}
