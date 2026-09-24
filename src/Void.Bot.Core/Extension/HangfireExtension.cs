using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Void.Bot.Core.Jobs;

namespace Void.Bot.Core.Extension;

public static class HangfireExtension
{
    extension(IApplicationBuilder app)
    {
        public void UseForgetJobs()
        {
            foreach (var type in typeof(IForgetJob).Assembly.GetTypes().Where(type => type.IsClass && typeof(IForgetJob).IsAssignableFrom(type)))
            {
                var job = (IForgetJob)app.ApplicationServices.GetRequiredService(type);

                if (typeof(IForgetJob).IsAssignableFrom(type))
                {
                    BackgroundJob.Enqueue(() => job.Execute());
                }
            }
        }

        public void UseDelayedJobs()
        {
            foreach (var type in typeof(IDelayedJob).Assembly.GetTypes().Where(type => type.IsClass && typeof(IDelayedJob).IsAssignableFrom(type)))
            {
                var job = (IDelayedJob)app.ApplicationServices.GetRequiredService(type);

                if (typeof(IDelayedJob).IsAssignableFrom(type))
                {
                    BackgroundJob.Schedule(() => job.Execute(), job.Delay);
                }
            }
        }

        public void UseRecurringJobs()
        {
            foreach (var type in typeof(IRecurringJob).Assembly.GetTypes().Where(type => type.IsClass && typeof(IRecurringJob).IsAssignableFrom(type)))
            {
                var job = (IRecurringJob)app.ApplicationServices.GetRequiredService(type);

                if (typeof(IRecurringJob).IsAssignableFrom(type))
                {
                    RecurringJob.AddOrUpdate(job.JobId, () => job.Execute(), job.CronExpression);
                }
            }
        }
    }
}