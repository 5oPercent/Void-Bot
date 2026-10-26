using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class ValidateEraArmorConfigurationRequestHandler : IRequestHandler<ValidateEraArmorConfigurationRequest, ValidateEraArmorConfigurationResponse>
{
    private readonly IEraArmorService _service;

    public ValidateEraArmorConfigurationRequestHandler(IEraArmorService service)
    {
        _service = service;
    }

    public async Task<ValidateEraArmorConfigurationResponse> Handle(IReceiveContext<ValidateEraArmorConfigurationRequest> context, CancellationToken cancellationToken)
    {
        return await _service.ValidateConfigurationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
