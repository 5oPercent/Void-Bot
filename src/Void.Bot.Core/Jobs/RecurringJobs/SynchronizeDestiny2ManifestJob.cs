using Hangfire;
using Mediator.Net;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Commands.Sync;

namespace Void.Bot.Core.Jobs.RecurringJobs;

public class SynchronizeDestiny2ManifestJob(IMediator mediator, Destiny2Setting destiny2Setting) : IRecurringJob
{
    public string JobId => nameof(SynchronizeDestiny2ManifestJob);

    public string CronExpression => Cron.Weekly(DayOfWeek.Wednesday, 3);

    public async Task Execute()
    {
        await mediator.SendAsync<SynchronizeDestiny2ManifestCommand, SynchronizeDestiny2ManifestResponse>(
            new SynchronizeDestiny2ManifestCommand
            {
                Languages = destiny2Setting.SyncLanguages,
                Components = destiny2Setting.SyncManifests,
                ForceSync = true
            }).ConfigureAwait(false);
    }
}