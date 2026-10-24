using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

/// <summary>读取当前装备目录、生成规则及共用背包容量，不生成或初始化资产。</summary>
public sealed class GetEraEquipmentManifestRequest : IRequest
{
}

/// <summary>当前发布目录；未初始化或格式无效时返回稳定错误码。</summary>
public sealed class GetEraEquipmentManifestResponse : EraResponse<EraEquipmentManifestDto>
{
}
