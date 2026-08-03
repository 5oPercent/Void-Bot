using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.MemoryStorage;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;
using Void.Bot.Api.ActionFilters;
using Void.Bot.Api.Authentication;
using Void.Bot.Api.HostedService;
using Void.Bot.Core;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Settings;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Dto;
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
            containerBuilder.RegisterType<InitializationHostedService>().As<IHostedService>().SingleInstance();
            containerBuilder.RegisterModule(new VoidBotModule(null, Log.Logger, configuration, typeof(VoidBotModule).Assembly));
        });
        
        return builder;
    }
    
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        
        var cacheSetting = new CacheSetting(configuration);
        var hangfireSetting = new HangfireSetting(configuration);
        var connectionSetting = new ConnectionStringSetting(configuration);
        
        services.AddMemoryCache();
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto;
        });
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

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        services.AddCustomValidationErrorHandling();
        
        services.AddHangfire(config =>
        {
            switch(hangfireSetting.UseStorage)
            {
                case CacheTypeEnum.Redis:
                    var option = new RedisStorageOptions { Db = cacheSetting.RedisDatabaseIndex };
                    config.UseRedisStorage(connectionSetting.Redis, option);
                    break;
                case CacheTypeEnum.Memory:
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

        app.UseForwardedHeaders();
        
        if (AppSetting.EnableScalar)
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        
        app.Use(async (context, next) =>
        {
            if (ThirdPartyAuthenticationBase.IsAuthorizationPath(context.Request.Path))
            {
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                context.Response.Headers.CacheControl = "no-store";
            }

            await next(context).ConfigureAwait(false);
        });
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        if (hangfireSetting.EnableDashboard)
        {
            var option = new DashboardOptions
            {
                Authorization = [
                    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                    {
                        SslRedirect = false,
                        RequireSsl = false,
                        LoginCaseSensitive = true,
                        Users =
                        [
                            new BasicAuthAuthorizationUser
                            {
                                Login = hangfireSetting.DashboardUser,
                                PasswordClear = hangfireSetting.DashboardPassword
                            }
                        ]
                    })
                ]
            };
            
            app.UseHangfireDashboard(hangfireSetting.DashboardPath, option);
        }
        app.UseForgetJobs();
        app.UseDelayedJobs();
        app.UseRecurringJobs();

        return app;
    }
    
    private static IServiceCollection AddCustomValidationErrorHandling(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e =>
                    {
                        var msg = e.Value!.Errors.First().ErrorMessage;
                        return $" -- {e.Key}: {msg} Severity: Error";
                    });

                var errorMessage = "Validation failed: \n" + string.Join("\n", errors);

                var response = new VoidBotResponse
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = errorMessage
                };

                return new JsonResult(response)
                {
                    StatusCode = 200 
                };
            };
        });

        return services;
    }
}
