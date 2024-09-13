using Autofac;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Redis.StackExchange;
using Serilog;
using Void.Bot.Core;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Enum;

namespace Void.Bot.Api;

public class Startup
{
    private IConfiguration Configuration { get; }
    
    private HangfireSetting HangfireSetting { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        HangfireSetting = new HangfireSetting(configuration);
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers();
        
        services.AddHangfire(configuration => configuration.UseMemoryStorage());
        
        services.AddHangfire(configuration =>
        {
            switch(HangfireSetting.UseStorage)
            {
                case HangfireStorageTypeEnum.Redis:
                    configuration.UseRedisStorage(new ConnectionStringSetting(Configuration).Redis);
                    break;
                case HangfireStorageTypeEnum.Memory:
                default:
                    configuration.UseMemoryStorage();
                    break;
            }
        });
        services.AddHangfireServer();
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new VoidBotModule(Log.Logger, typeof(VoidBotModule).Assembly));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(ep => ep.MapControllers());

        if (HangfireSetting.EnableDashboard)
        {
            app.UseHangfireDashboard();
        }
        app.UseRecurringJobs();
    }
}