using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>谐振技能草稿管理消息，保存不直接发布。</summary>
public sealed class GetEraDraftLatticeRequest : IRequest
{
    /// <summary>路由写入的稳定对象 ID。</summary>
    public string Id { get; set; } = "";
}

/// <summary>谐振技能草稿操作结果。</summary>
public sealed class GetEraDraftLatticeResponse : EraResponse<EraResonanceDraftItem<LatticeDefinition>>
{
}
