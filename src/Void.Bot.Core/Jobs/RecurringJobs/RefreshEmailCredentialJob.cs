using Mediator.Net;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Commands.Email;

namespace Void.Bot.Core.Jobs.RecurringJobs;

public sealed class RefreshEmailCredentialJob(IMediator mediator) : IRecurringJob
{
    public string JobId => nameof(RefreshEmailCredentialJob);

    public string CronExpression => "0 3 * * *";

    public async Task Execute()
    {
        try
        {
            await mediator.SendAsync<RefreshEmailCredentialCommand, RefreshEmailCredentialResponse>(
                new RefreshEmailCredentialCommand(), CancellationToken.None).ConfigureAwait(false);
        }
        catch (EmailReauthorizationRequiredException)
        {
            // Reauthorization failures are persisted and logged by the credential manager.
        }
    }
}
