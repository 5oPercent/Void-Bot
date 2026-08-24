using System.Net;
using Mediator.Net;
using Serilog;
using Void.Bot.Message.Commands.Email;

namespace Void.Bot.Core.Jobs.RecurringJobs;

public sealed class RefreshEmailCredentialJob(
    IMediator mediator,
    ILogger logger) : IRecurringJob
{
    public string JobId => nameof(RefreshEmailCredentialJob);

    public string CronExpression => "0 3 * * *";

    public async Task Execute()
    {
        var response = await mediator.SendAsync<RefreshEmailCredentialCommand, RefreshEmailCredentialResponse>(
            new RefreshEmailCredentialCommand(), CancellationToken.None).ConfigureAwait(false);
        if (response.Code == default)
        {
            return;
        }

        if (response.Code == HttpStatusCode.Unauthorized)
        {
            logger.Warning("Email credential requires reauthorization");
            return;
        }

        logger.Warning("Scheduled email credential refresh failed: {ErrorCode}", response.Message);
        throw new InvalidOperationException("Scheduled email credential refresh failed.");
    }
}
