using System.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public sealed class ApproveEraModularActivityHandler(IEraModularActivityService service)
    : ICommandHandler<ApproveEraModularActivity, ApproveEraModularActivityResponse>
{
    public async Task<ApproveEraModularActivityResponse> Handle(IReceiveContext<ApproveEraModularActivity> context,
        CancellationToken cancellationToken)
    {
        var request = context.Message;
        var id = await service.ApproveAsync(request.WorldInstanceId, request, cancellationToken).ConfigureAwait(false);
        return new ApproveEraModularActivityResponse { Code = HttpStatusCode.OK, Data = id };
    }
}

public sealed class ClaimEraModularActivityHandler(IEraModularActivityService service)
    : ICommandHandler<ClaimEraModularActivity, ClaimEraModularActivityResponse>
{
    public async Task<ClaimEraModularActivityResponse> Handle(IReceiveContext<ClaimEraModularActivity> context,
        CancellationToken cancellationToken)
    {
        var request = context.Message;
        var selection = await service.ClaimAsync(request.WorldInstanceId, request, cancellationToken).ConfigureAwait(false);
        return new ClaimEraModularActivityResponse { Code = HttpStatusCode.OK, Data = selection };
    }
}
