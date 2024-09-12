using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Void.Bot.Core.Jobs;

namespace Void.Bot.Core.Extension;

public static class HangfireExtension
{
    public static void UseRecurringJobs(this IApplicationBuilder app)
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