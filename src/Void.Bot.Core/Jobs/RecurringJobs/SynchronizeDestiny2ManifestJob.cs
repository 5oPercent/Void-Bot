using Hangfire;
using Mediator.Net;
using Void.Bot.Message.Commands.Sync;

namespace Void.Bot.Core.Jobs.RecurringJobs;

public class SynchronizeDestiny2ManifestJob(IMediator mediator) : IRecurringJob
{
    public string JobId => nameof(SynchronizeDestiny2ManifestJob);

    public string CronExpression => Cron.Daily(3);

    public async Task Execute()
    {
        await mediator.SendAsync<SynchronizeDestiny2ManifestCommand, SynchronizeDestiny2ManifestResponse>(
            new SynchronizeDestiny2ManifestCommand()).ConfigureAwait(false);
    }
}