using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Redis.StackExchange;
using Scalar.AspNetCore;
using Serilog;
using Void.Bot.Api.ActionFilters;
using Void.Bot.Core;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Enum;

namespace Void.Bot.Api;

public static class HostExtension
{
    public static WebApplicationBuilder ConfigureHost(this WebApplicationBuilder builder)
    {
        var host = builder.Host;
        var configuration = builder.Configuration;

        host.UseSerilog();
        host.ConfigureLogging(l => l.AddSerilog(Log.Logger));
        host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            containerBuilder.RegisterModule(new VoidBotModule(Log.Logger, configuration, typeof(VoidBotModule).Assembly));
        });
        
        return builder;
    }
    
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        
        var hangfireSetting = new HangfireSetting(configuration);
        var connectionSetting = new ConnectionStringSetting(configuration);
        
        services.AddAuthorization();
        services.AddOpenApi();
        services.AddControllers(options =>
        {
            options.Filters.Add<UnifyResponseFilter>();
            options.Filters.Add<UnifyResponseExceptionFilter>();
        });
        
        services.AddHangfire(config =>
        {
            switch(hangfireSetting.UseStorage)
            {
                case HangfireStorageTypeEnum.Redis:
                    config.UseRedisStorage(connectionSetting.Redis);
                    break;
                case HangfireStorageTypeEnum.Memory:
                default:
                    config.UseMemoryStorage();
                    break;
            }
        });
        services.AddHangfireServer();

        return builder;
    }
    
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        var configuration = app.Configuration;
        var hangfireSetting = new HangfireSetting(configuration);
        
        if (app.Environment.IsDevelopment())
        {
            app.UseStaticFiles();
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(ep => ep.MapControllers());
        
        if (hangfireSetting.EnableDashboard)
        {
            app.UseHangfireDashboard();
        }
        app.UseForgetJobs();
        app.UseDelayedJobs();
        app.UseRecurringJobs();

        return app;
    }
}