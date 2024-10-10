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
    private readonly IConfiguration _configuration;

    private readonly HangfireSetting _hangfireSetting;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
        _hangfireSetting = new HangfireSetting(_configuration);
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers();
        
        services.AddHangfire(configuration =>
        {
            switch(_hangfireSetting.UseStorage)
            {
                case HangfireStorageTypeEnum.Redis:
                    configuration.UseRedisStorage(new ConnectionStringSetting(_configuration).Redis);
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
        builder.RegisterModule(new VoidBotModule(Log.Logger, _configuration, typeof(VoidBotModule).Assembly));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();

            app.UseStaticFiles();
            
            app.UseSwaggerUI(c =>
            {
                c.InjectStylesheet("/swagger-dark-theme.css");
            });
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(ep => ep.MapControllers());
        
        if (_hangfireSetting.EnableDashboard)
        {
            app.UseHangfireDashboard();
        }
        app.UseForgetJobs();
        app.UseDelayedJobs();
        app.UseRecurringJobs();
    }
}