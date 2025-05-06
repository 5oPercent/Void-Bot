using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using Serilog;
using Void.Bot.Api.ActionFilters;
using Void.Bot.Api.Authentication;
using Void.Bot.Core;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Enum;

namespace Void.Bot.Api.Extension;

public static class HostExtension
{
    public static WebApplicationBuilder ConfigureHost(this WebApplicationBuilder builder)
    {
        var host = builder.Host;
        var configuration = builder.Configuration;

        host.UseSerilog();
        host.UseSerilog(Log.Logger);
        host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            containerBuilder.RegisterModule(new VoidBotModule(null, Log.Logger, configuration, typeof(VoidBotModule).Assembly));
        });
        
        return builder;
    }
    
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        
        var hangfireSetting = new HangfireSetting(configuration);
        var connectionSetting = new ConnectionStringSetting(configuration);
        
        services.AddHttpContextAccessor();
        services.AddAuthentication("AuthenticationSchemes")
            .AddPolicyScheme("AuthenticationSchemes", "ApiKey or Jwt", options =>
            {
                options.ForwardDefaultSelector = context => context.Request.Headers.ContainsKey(ApiKeyAuthenticationOptions.SchemeHeader)
                    ? ApiKeyAuthenticationOptions.AuthenticationScheme
                    : JwtBearerDefaults.AuthenticationScheme;
            })
            .AddApiKeyScheme()
            .AddJwtScheme();
        
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>();
        });
        
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                ApiKeyAuthenticationOptions.AuthenticationScheme
            ).RequireAuthenticatedUser().Build();
        });

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
        
        if (AppSetting.EnableScalar)
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
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