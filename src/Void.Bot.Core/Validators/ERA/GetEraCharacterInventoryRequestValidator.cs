using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraCharacterInventoryRequestValidator : FluentMessageValidator<GetEraCharacterInventoryRequest>
{
    public GetEraCharacterInventoryRequestValidator()
    {
        RuleFor(request => request.CharacterId).NotEmpty();
        // 槽位协议值由 Service 校验，保留稳定的 era.equipment_invalid_slot 错误码。
    }
}
